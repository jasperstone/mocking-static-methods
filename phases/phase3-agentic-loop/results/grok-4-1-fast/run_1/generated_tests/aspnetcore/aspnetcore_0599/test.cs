using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.IIS.Core;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core.Tests
{
    public class IISHttpServerLoggingTests
    {
        [Fact]
        public void HandleRequest_WhenContextCreationFails_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IISHttpServer>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected exception in static IISHttpServer.HandleRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var server = CreateServer(mockLogger.Object);

            var gcHandle = GCHandle.Alloc(server);
            try
            {
                var pvRequestContext = GCHandle.ToIntPtr(gcHandle);
                var pInProcessHandler = IntPtr.Zero;

                var method = typeof(IISHttpServer)
                    .GetMethod("HandleRequest", BindingFlags.NonPublic | BindingFlags.Static)!;

                // Act - throws NullReferenceException because _iisContextFactory is null
                method.Invoke(null, new object[] { pInProcessHandler, pvRequestContext });
            }
            finally
            {
                gcHandle.Free();
            }

            // Assert
            mockLogger.Verify();
        }

        [Fact]
        public void HandleShutdown_WhenApplicationLifetimeFails_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IISHttpServer>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected exception in IISHttpServer.HandleShutdown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var mockLifetime = new Mock<IHostApplicationLifetime>();
            mockLifetime.Setup(m => m.StopApplication()).Throws(new InvalidOperationException("test exception"));

            var server = CreateServer(mockLogger.Object, mockLifetime.Object);

            var gcHandle = GCHandle.Alloc(server);
            try
            {
                var pvRequestContext = GCHandle.ToIntPtr(gcHandle);

                var method = typeof(IISHttpServer)
                    .GetMethod("HandleShutdown", BindingFlags.NonPublic | BindingFlags.Static)!;

                // Act
                method.Invoke(null, new object[] { pvRequestContext });
            }
            finally
            {
                gcHandle.Free();
            }

            // Assert
            mockLogger.Verify();
        }

        private static IISHttpServer CreateServer(ILogger<IISHttpServer> logger, IHostApplicationLifetime lifetime = null)
        {
            var mockAuth = new Mock<IAuthenticationSchemeProvider>();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
            
            var mockPoolFactory = new Mock<IMemoryPoolFactory<byte>>();
            mockPoolFactory.Setup(f => f.Create(It.IsAny<MemoryPoolOptions>()))
                          .Returns(System.Buffers.MemoryPool<byte>.Shared);
            
            var options = new OptionsWrapper<IISServerOptions>(new IISServerOptions());

            var dummyHandle = new NativeSafeHandle(new IntPtr(123), ownsHandle: false);
            var nativeApp = new IISNativeApplication(dummyHandle);

            return new IISHttpServer(
                nativeApp,
                lifetime ?? new Mock<IHostApplicationLifetime>().Object,
                mockAuth.Object,
                config,
                mockPoolFactory.Object,
                options,
                logger);
        }
    }
}
