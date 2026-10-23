using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Critical_CallsLogCritical()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Critical error occurred";

            // Act
            AbpLoggerExtensions.LogWithLevel(mockLogger.Object, LogLevel.Critical, message);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Critical_WithException_CallsLogCriticalWithException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Critical error with exception";
            var exception = new Exception("Test exception");

            // Act
            AbpLoggerExtensions.LogWithLevel(mockLogger.Object, LogLevel.Critical, message, exception);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
