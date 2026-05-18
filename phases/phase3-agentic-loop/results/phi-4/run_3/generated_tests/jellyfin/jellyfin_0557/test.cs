using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Extensions.Tests
{
    public class LoggerExtensionsTests
    {
        private const string ErrorMessage = "Error renaming legacy user database to 'users.db.old'";

        [Fact]
        public void LogError_WhenLoggerIsEnabled_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            var logger = loggerMock.Object;

            var exception = new IOException("Test exception");

            // Act
            logger.LogError(exception, ErrorMessage);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s == ErrorMessage), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogError_WhenLoggerIsNotEnabled_DoesNotLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(false);
            var logger = loggerMock.Object;

            var exception = new IOException("Test exception");

            // Act
            logger.LogError(exception, ErrorMessage);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
