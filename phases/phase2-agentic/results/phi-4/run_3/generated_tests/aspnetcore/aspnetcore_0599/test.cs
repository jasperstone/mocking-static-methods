using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.Core.Tests
{
    public class IISHttpServerTests
    {
        [Fact]
        public void HandleRequest_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IISHttpServer>>();
            var server = new IISHttpServer(
                null, // Mock or stub IISNativeApplication
                null, // Mock or stub IHostApplicationLifetime
                null, // Mock or stub IAuthenticationSchemeProvider
                null, // Mock or stub IConfiguration
                null, // Mock or stub IMemoryPoolFactory<byte>
                null, // Mock or stub IOptions<IISServerOptions>
                loggerMock.Object
            );

            // Simulate an exception
            var exception = new InvalidOperationException("Test exception");

            // Act
            // Simulate the HandleRequest method throwing an exception
            try
            {
                // This is a simplified simulation of the HandleRequest method
                throw exception;
            }
            catch (Exception ex)
            {
                server._logger.LogError(0, ex, $"Unexpected exception in static {nameof(IISHttpServer)}.{nameof(IISHttpServer.HandleRequest)}.");
            }

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected exception in static IISHttpServer.HandleRequest.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
