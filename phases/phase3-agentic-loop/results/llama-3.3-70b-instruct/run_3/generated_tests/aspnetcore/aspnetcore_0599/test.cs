using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Tests
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
                new ConfigurationBuilder().Build(),
                new MemoryPoolFactory(),
                Options.Create(new IISServerOptions()),
                loggerMock.Object
            );

            var exception = new Exception("Test exception");
            var pvRequestContext = GCHandle.Alloc(server);

            // Act and Assert
            try
            {
                IISHttpServer.HandleRequest(IntPtr.Zero, pvRequestContext.AddrOfPinnedObject());
            }
            catch
            {
            }
            loggerMock.Verify(l => l.LogError(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
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
                new ConfigurationBuilder().Build(),
                new MemoryPoolFactory(),
                Options.Create(new IISServerOptions()),
                loggerMock.Object
            );

            var exception = new Exception("Test exception");
            var pvRequestContext = GCHandle.Alloc(server);

            // Act and Assert
            try
            {
                IISHttpServer.HandleShutdown(pvRequestContext.AddrOfPinnedObject());
            }
            catch
            {
            }
            loggerMock.Verify(l => l.LogError(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
