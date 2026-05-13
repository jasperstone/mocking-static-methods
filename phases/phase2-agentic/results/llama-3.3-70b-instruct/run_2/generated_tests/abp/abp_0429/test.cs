using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Volo.Abp.Core.Tests
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_LogCritical_CallsLogCriticalOnLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logLevel = LogLevel.Critical;
            var message = "Test message";
            var exception = new Exception("Test exception");

            // Act
            AbpLoggerExtensions.LogWithLevel(loggerMock.Object, logLevel, message, exception);

            // Assert
            loggerMock.Verify(l => l.LogCritical(exception, message), Times.Once);
        }

        [Fact]
        public void LogException_LogsExceptionWithDefaultLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception);

            // Assert
            loggerMock.Verify(l => l.LogCritical(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogException_LogsExceptionWithSpecifiedLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var logLevel = LogLevel.Error;

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception, logLevel);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
