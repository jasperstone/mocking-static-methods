using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core.Tests
{
    public class IISHttpServerTests
    {
        [Fact]
        public void HandleRequest_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IISHttpServer>>();
            var server = new IISHttpServerMock(loggerMock.Object);

            // Create a GCHandle to the server instance and get IntPtr
            var gch = GCHandle.Alloc(server);
            var ptr = GCHandle.ToIntPtr(gch);

            // We will pass an invalid pInProcessHandler to cause an exception in HandleRequest
            IntPtr invalidHandler = IntPtr.Zero;

            // Act
            var result = IISHttpServer.HandleRequest(invalidHandler, ptr);

            // Assert
            Assert.Equal(NativeMethods.REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST, result);
            loggerMock.Verify(
                x => x.LogError(
                    0,
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("Unexpected exception in static IISHttpServer.HandleRequest."))),
                Times.Once);

            gch.Free();
        }

        // Minimal mock subclass to allow setting logger and avoid null reference
        private class IISHttpServerMock : IISHttpServer
        {
            public IISHttpServerMock(ILogger<IISHttpServer> logger)
                : base(
                    nativeApplication: null!,
                    applicationLifetime: null!,
                    authentication: null!,
                    configuration: null!,
                    memoryPoolFactory: new TestMemoryPoolFactory(),
                    options: Microsoft.Extensions.Options.Options.Create(new IISServerOptions()),
                    logger: logger)
            {
            }
        }

        // Minimal memory pool factory for constructor
        private class TestMemoryPoolFactory : IMemoryPoolFactory<byte>
        {
            public MemoryPool<byte> Create(MemoryPoolOptions options) => MemoryPool<byte>.Shared;
        }
    }

    // Minimal NativeMethods stub for REQUEST_NOTIFICATION_STATUS enum
    internal static class NativeMethods
    {
        public enum REQUEST_NOTIFICATION_STATUS
        {
            RQ_NOTIFICATION_PENDING = 0,
            RQ_NOTIFICATION_FINISH_REQUEST = 1
        }
    }
}
