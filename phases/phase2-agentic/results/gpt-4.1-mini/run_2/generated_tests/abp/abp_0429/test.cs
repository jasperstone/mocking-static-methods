using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Calls_LogCritical_With_Exception_And_Message()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var message = "Test message";

            // Act
            loggerMock.Object.LogWithLevel(LogLevel.Critical, message, exception);

            // Assert
            loggerMock.Verify(
                x => x.LogCritical(exception, message),
                Times.Once);
        }

        [Fact]
        public void LogWithLevel_Calls_LogCritical_Without_Exception()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Critical message";

            // Act
            loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

            // Assert
            loggerMock.Verify(
                x => x.LogCritical(message),
                Times.Once);
        }
    }
}
