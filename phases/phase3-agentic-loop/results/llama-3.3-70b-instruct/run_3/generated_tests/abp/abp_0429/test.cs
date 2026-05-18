using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Volo.Abp.Core.Tests
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_LogCritical_WithException_CallsLogCriticalOnLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var message = "Test message";

            // Act
            AbpLoggerExtensions.LogWithLevel(loggerMock.Object, LogLevel.Critical, message, exception);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void LogException_LogCritical_WithException_CallsLogCriticalOnLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception, LogLevel.Critical);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
