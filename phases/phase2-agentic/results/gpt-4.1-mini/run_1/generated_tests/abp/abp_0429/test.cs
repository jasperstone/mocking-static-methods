using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Calls_LogCritical_When_LogLevel_Is_Critical_Without_Exception()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Critical error message";

            // Act
            loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

            // Assert
            loggerMock.Verify(l => l.LogCritical(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Calls_LogCritical_When_LogLevel_Is_Critical_With_Exception()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Critical error message";
            var exception = new Exception("Critical exception");

            // Act
            loggerMock.Object.LogWithLevel(LogLevel.Critical, message, exception);

            // Assert
            loggerMock.Verify(l => l.LogCritical(exception, message), Times.Once);
        }
    }
}
