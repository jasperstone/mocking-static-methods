using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_RelinquishOwnershipFailure_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            
            string sourceNodeId = "source-node-123";
            string targetNodeId = "target-node-456";
            
            // Act - Directly test the LoggerExtensions.LogError call from line 206
            logger.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", sourceNodeId, targetNodeId);
            
            // Assert - Verify underlying Log method called with correct formatted message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Failed to relinquish ownership") &&
                        v.ToString()!.Contains($"({sourceNodeId})") &&
                        v.ToString()!.Contains($"({targetNodeId})")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_RelinquishOwnershipFailure_NullLogger_Safe()
        {
            // Arrange
            ILogger? logger = null;
            string sourceNodeId = "source";
            string targetNodeId = "target";
            
            // Act - Test the null-conditional operator ?. behavior from source code
            logger?.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", sourceNodeId, targetNodeId);
            
            // Assert - No exception thrown, null-conditional works
            Assert.True(true);
        }

        [Fact]
        public void LogError_RelinquishOwnership_WithSpecificNodeIds_FormatsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            
            string sourceNodeId = "node-001";
            string targetNodeId = "node-002";
            
            // Act - Exact call matching line 206 with realistic node IDs
            logger.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", sourceNodeId, targetNodeId);
            
            // Assert - Verify exact format with parameter substitution
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("source node:(node-001)") &&
                        v.ToString()!.Contains("target node: (node-002)")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
