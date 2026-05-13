using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Volo.Abp.Core.Tests
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_LogCritical_CallsLogCritical()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logLevel = LogLevel.Critical;
            var message = "Test message";

            // Act
            loggerMock.Object.LogWithLevel(logLevel, message);

            // Assert
            loggerMock.Verify(l => l.LogCritical(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_LogCritical_WithException_CallsLogCritical()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logLevel = LogLevel.Critical;
            var message = "Test message";
            var exception = new Exception();

            // Act
            loggerMock.Object.LogWithLevel(logLevel, message, exception);

            // Assert
            loggerMock.Verify(l => l.LogCritical(exception, message), Times.Once);
        }

        [Fact]
        public void LogException_LogsException_WithDefaultLogLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception();

            // Act
            loggerMock.Object.LogException(exception);

            // Assert
            loggerMock.Verify(l => l.LogWithLevel(It.IsAny<LogLevel>(), exception.Message, exception), Times.Once);
        }
    }
}
