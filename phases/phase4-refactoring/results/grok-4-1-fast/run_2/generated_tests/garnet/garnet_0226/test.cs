using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_ForegroundMessage_CallsLogWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var message = "Initiating foreground checkpoint retrieval";

            // Act
            ((ILogger)loggerMock.Object).LogInformation(message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_LoggerDisabled_DoesNotCallLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(false);
            var message = "Initiating foreground checkpoint retrieval";

            // Act
            ((ILogger)loggerMock.Object).LogInformation(message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogInformation_NullLogger_DoesNotThrow()
        {
            // Arrange & Act & Assert
            ILogger? logger = null;
            var message = "Initiating foreground checkpoint retrieval";
            Assert.Same(logger, logger?.LogInformation(message));
        }
    }
}
