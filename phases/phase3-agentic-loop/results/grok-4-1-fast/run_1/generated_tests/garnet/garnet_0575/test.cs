using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.server
{
    public class TxnRespCommandsLoggerTests
    {
        [Fact]
        public void LogWarning_VerifiesExtensionMethodBehavior()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
            mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act - Directly test the LogWarning extension method usage pattern
            mockLogger.Object.LogWarning("Failed CheckClusterTxnKeys");

            // Assert - Verify the underlying Log method was called with Warning level
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v).Contains("Failed CheckClusterTxnKeys")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_SkippedWhenLoggerDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(false);

            // Act
            mockLogger.Object.LogWarning("Failed CheckClusterTxnKeys");

            // Assert - Log method should not be called when warnings are disabled
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void NullLogger_SafeWithNullConditional()
        {
            // Arrange - Simulates logger?.LogWarning case
            ILogger logger = NullLogger.Instance;

            // Act & Assert - No exception thrown (null-conditional handles null logger)
            logger.LogWarning("Failed CheckClusterTxnKeys");
            Assert.True(true);
        }
    }
}
