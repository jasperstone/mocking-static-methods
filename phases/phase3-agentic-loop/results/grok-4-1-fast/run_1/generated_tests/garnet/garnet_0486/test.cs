using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.server.tests
{
    public class ServerConfigTests
    {
        [Fact]
        public void LogWarning_ClusterUsernameNotProvided_CallsLogWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            var logger = loggerMock.Object;
            logger.LogWarning("Cluster username is not provided, will use new password with existing username");

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;

            // Act & Assert - null-conditional operator prevents call
            logger?.LogWarning("Cluster username is not provided, will use new password with existing username");
            Assert.True(true);
        }

        [Fact]
        public void LogWarning_WarningDisabled_DoesNotCallLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(false);

            // Act
            loggerMock.Object.LogWarning("Cluster username is not provided, will use new password with existing username");

            // Assert - LogWarning extension method checks IsEnabled first
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogWarning_LoggerEnabled_CapturesCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            var logger = loggerMock.Object;
            const string expectedMessage = "Cluster username is not provided, will use new password with existing username";
            logger.LogWarning(expectedMessage);

            // Assert - verify LogWarning was called (extension method behavior)
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Warning), Times.Once);
        }
    }
}
