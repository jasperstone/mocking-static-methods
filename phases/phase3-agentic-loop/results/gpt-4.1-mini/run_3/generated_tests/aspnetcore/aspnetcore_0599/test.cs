using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Tests
{
    public class IISHttpServerTests
    {
        [Fact]
        public void HandleRequest_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IISHttpServer>>();
            var nativeAppMock = new Mock<IISNativeApplication>();
            var lifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var authSchemeProviderMock = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
            var configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var memoryPoolFactoryMock = new Mock<IMemoryPoolFactory<byte>>();
            var memoryPoolMock = new Mock<System.Buffers.MemoryPool<byte>>();
            var optionsMock = new Mock<IOptions<IISServerOptions>>();

            memoryPoolFactoryMock.Setup(f => f.Create(It.IsAny<MemoryPoolOptions>()))
                .Returns(memoryPoolMock.Object);

            optionsMock.Setup(o => o.Value).Returns(new IISServerOptions());

            // Create IISHttpServer instance
            var server = new IISHttpServer(
                nativeAppMock.Object,
                lifetimeMock.Object,
                authSchemeProviderMock.Object,
                configurationMock.Object,
                memoryPoolFactoryMock.Object,
                optionsMock.Object,
                loggerMock.Object);

            // Create a proxy that throws on CreateHttpContext
            var proxy = new ThrowingIISContextFactory();

            // Set the private _iisContextFactory field to the proxy
            var iisContextFactoryField = typeof(IISHttpServer).GetField("_iisContextFactory", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(iisContextFactoryField);
            iisContextFactoryField.SetValue(server, proxy);

            // Create a GCHandle to the server instance
            var gch = GCHandle.Alloc(server);

            try
            {
                // Get the private static HandleRequest method
                var handleRequestMethod = typeof(IISHttpServer).GetMethod("HandleRequest", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.NotNull(handleRequestMethod);

                // Act
                var result = (NativeMethods.REQUEST_NOTIFICATION_STATUS)handleRequestMethod.Invoke(null, new object[] { IntPtr.Zero, GCHandle.ToIntPtr(gch) });

                // Assert
                Assert.Equal(NativeMethods.REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST, result);

                // Verify that LogError was called once
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected exception in static IISHttpServer.HandleRequest.")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                gch.Free();
            }
        }

        private class ThrowingIISContextFactory
        {
            public object CreateHttpContext(NativeSafeHandle safeHandle)
            {
                throw new InvalidOperationException("Test exception");
            }
        }
    }

    // Minimal stubs for NativeMethods and NativeSafeHandle to compile the test
    internal static class NativeMethods
    {
        public enum REQUEST_NOTIFICATION_STATUS
        {
            RQ_NOTIFICATION_PENDING = 0,
            RQ_NOTIFICATION_FINISH_REQUEST = 1
        }
    }

    internal class NativeSafeHandle : IDisposable
    {
        public NativeSafeHandle(IntPtr handle) { }
        public void Dispose() { }
    }

    // Minimal interfaces to satisfy dependencies
    internal interface IMemoryPoolFactory<T>
    {
        System.Buffers.MemoryPool<T> Create(MemoryPoolOptions options);
    }

    internal class MemoryPoolOptions
    {
        public string? Owner { get; set; }
    }
}
