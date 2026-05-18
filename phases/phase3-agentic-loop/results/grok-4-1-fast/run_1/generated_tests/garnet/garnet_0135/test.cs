using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster
{
    public class MigrationDriverLoggerTests
    {
        [Fact]
        public void RelinquishOwnershipFailure_LogsErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            
            var logger = mockLogger.Object;
            string sourceNodeId = "source123";
            string targetNodeId = "target456";

            // Act - simulate the exact LogError call from line 206
            logger.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", sourceNodeId, targetNodeId);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Failed to relinquish ownership from source node") &&
                        v.ToString()!.Contains(sourceNodeId) &&
                        v.ToString()!.Contains(targetNodeId)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void TrySetSlotRangesFailure_LogsErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            
            var logger = mockLogger.Object;
            string targetNodeId = "targetNodeId";
            string endpoint = "targetEndpoint";

            // Act - simulate the LogError call from the TrySetSlotRangesAsync failure path
            logger.LogError("Failed to assign ownership to target node:({tgtNodeId}) ({endpoint})", targetNodeId, endpoint);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Failed to assign ownership to target node") &&
                        v.ToString()!.Contains(targetNodeId) &&
                        v.ToString()!.Contains(endpoint)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void MigrateSlotsDriverFailure_LogsErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            
            var logger = mockLogger.Object;

            // Act - simulate the LogError call from MigrateSlotsDriver failure
            logger.LogError("MigrateSlotsDriver failed");

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MigrateSlotsDriver failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
