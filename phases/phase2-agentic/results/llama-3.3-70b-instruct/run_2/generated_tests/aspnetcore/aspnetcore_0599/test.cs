using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
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
                new MemoryPoolFactory<byte>(),
                Options.Create(new IISServerOptions()),
                loggerMock.Object
            );

            var exception = new Exception("Test exception");
            var pvRequestContext = GCHandle.Alloc(server);

            // Act and Assert
            Assert.Throws<Exception>(() => IISHttpServer.HandleRequest(IntPtr.Zero, (IntPtr)pvRequestContext));
            loggerMock.Verify(l => l.LogError(0, exception, $"Unexpected exception in static {nameof(IISHttpServer)}.{nameof(IISHttpServer.HandleRequest)}."));
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
                new MemoryPoolFactory<byte>(),
                Options.Create(new IISServerOptions()),
                loggerMock.Object
            );

            var exception = new Exception("Test exception");
            var pvRequestContext = GCHandle.Alloc(server);

            // Act and Assert
            Assert.Throws<Exception>(() => IISHttpServer.HandleShutdown((IntPtr)pvRequestContext));
            loggerMock.Verify(l => l.LogError(0, exception, $"Unexpected exception in {nameof(IISHttpServer)}.{nameof(IISHttpServer.HandleShutdown)}."));
        }
    }
}
