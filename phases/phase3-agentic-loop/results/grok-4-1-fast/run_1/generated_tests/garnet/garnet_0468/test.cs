using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text;
using Xunit;

namespace Garnet.server.Resp.Vector.Tests
{
    public class VectorManagerLoggerExtensionsTests
    {
        [Fact]
        public void LogError_CalledWithExceptionAndKey_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VectorManager>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            var logger = loggerMock.Object;
            var exception = new InvalidOperationException("Test exception");
            var keyString = "testkey";

            // Act
            logger.LogError(exception, "Attempt at normal cleanup of {key} failed", keyString);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of testkey failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
