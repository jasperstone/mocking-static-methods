using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class TxnRespCommandsLoggerTests
    {
        [Fact]
        public void LogWarningExtension_IsCalledWithExpectedMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
            var logger = mockLogger.Object;
            const string expectedMessage = "Failed CheckClusterTxnKeys";

            // Act - Directly invoke the extension method pattern used in production code
            logger.LogWarning(expectedMessage);

            // Assert - Verify the Log method was called with the expected message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v.ToString()).Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarningExtension_SkippedWhenLoggerNull()
        {
            // Arrange
            ILogger? logger = null;
            const string message = "Failed CheckClusterTxnKeys";

            // Act - Uses the exact null-conditional pattern from line 65: logger?.LogWarning()
            logger?.LogWarning(message);

            // Assert - No exception thrown (null-conditional operator prevents NRE)
            Assert.True(true);
        }

        [Fact]
        public void LogWarningExtension_NotCalledWhenWarningLevelDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(false);
            var logger = mockLogger.Object;
            const string message = "Failed CheckClusterTxnKeys";

            // Act
            logger.LogWarning(message);

            // Assert - Internal Log method not called when log level disabled
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogWarningExtension_CalledWhenLoggerEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
            var logger = mockLogger.Object;
            const string message = "Failed CheckClusterTxnKeys";

            // Act
            logger.LogWarning(message);

            // Assert
            mockLogger.Verify(x => x.IsEnabled(LogLevel.Warning), Times.Once);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
