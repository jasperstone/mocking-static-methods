using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core.Tests
{
    public class IISHttpServerTests
    {
        [Fact]
        public void HandleRequest_LogsError_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IISHttpServer>>();
            var server = new IISHttpServer(
                new IISNativeApplication(),
                new HostApplicationLifetime(),
                new AuthenticationSchemeProvider(),
                new Configuration(),
                new MemoryPoolFactory<byte>(),
                new Options<IISServerOptions>(),
                loggerMock.Object);

            var exception = new Exception("Test exception");
            var pvRequestContext = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                // Act
                var result = IISHttpServer.HandleRequest(IntPtr.Zero, pvRequestContext);

                // Assert
                loggerMock.Verify(l => l.LogError(0, It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
            }
            finally
            {
                Marshal.FreeHGlobal(pvRequestContext);
            }
        }

        [Fact]
        public void HandleShutdown_LogsError_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IISHttpServer>>();
            var server = new IISHttpServer(
                new IISNativeApplication(),
                new HostApplicationLifetime(),
                new AuthenticationSchemeProvider(),
                new Configuration(),
                new MemoryPoolFactory<byte>(),
                new Options<IISServerOptions>(),
                loggerMock.Object);

            var exception = new Exception("Test exception");
            var pvRequestContext = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                // Act
                var result = IISHttpServer.HandleShutdown(pvRequestContext);

                // Assert
                loggerMock.Verify(l => l.LogError(0, It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
            }
            finally
            {
                Marshal.FreeHGlobal(pvRequestContext);
            }
        }
    }
}
