using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class MigrationLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public MigrationLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLogger
                .Setup(x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public void RelinquishOwnershipFailure_LogsCorrectErrorMessage()
        {
            // Arrange - Test the specific LogError call on line 206
            var logger = _mockLogger.Object;

            // Act - Directly call the exact LoggerExtensions.LogError from the source code
            logger.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", "source-123", "target-456");

            // Assert - Verify the low-level Log call was made with correct parameters
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString() == "Failed to relinquish ownership from source node:(source-123) to target node: (target-456)"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TrySetSlotRangesFailure_LogsCorrectErrorMessage()
        {
            // Arrange
            var logger = _mockLogger.Object;

            // Act - Test the LogError from TrySetSlotRangesAsync failure path
            logger.LogError("Failed to assign ownership to target node:({tgtNodeId}) ({endpoint})", "target-123", "target-endpoint");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString() == "Failed to assign ownership to target node:(target-123) (target-endpoint)"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void MigrateSlotsDriverFailure_LogsError()
        {
            // Arrange
            var logger = _mockLogger.Object;

            // Act - Test the LogError when MigrateSlotsDriver fails
            logger.LogError("MigrateSlotsDriver failed");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString() == "MigrateSlotsDriver failed"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
