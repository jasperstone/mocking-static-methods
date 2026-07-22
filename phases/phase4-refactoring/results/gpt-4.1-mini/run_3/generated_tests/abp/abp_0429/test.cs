using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Calls_LogCritical_When_LogLevel_Critical_Without_Exception()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Critical error message";

            // Act
            loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

            // Assert
            // We cannot verify extension method calls directly, so verify the underlying Log method was called with LogLevel.Critical
            loggerMock.Verify(l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWithLevel_Calls_LogCritical_When_LogLevel_Critical_With_Exception()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Critical error message";
            var exception = new Exception("Critical exception");

            // Act
            loggerMock.Object.LogWithLevel(LogLevel.Critical, message, exception);

            // Assert
            // We cannot verify extension method calls directly, so verify the underlying Log method was called with LogLevel.Critical and the exception
            loggerMock.Verify(l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
