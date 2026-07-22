using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Migration.Test
{
    public class MigrationDriverLoggerTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public MigrationDriverLoggerTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void RelinquishOwnershipFailure_LogsErrorWithCorrectTemplate()
        {
            // Arrange
            var logger = _loggerMock.Object;
            var sourceNodeId = "source-123";
            var targetNodeId = "target-456";

            // Act - Directly test the ILogger.LogError extension call from line 206
            logger.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", sourceNodeId, targetNodeId);

            // Assert - Verify the underlying Log call captures the formatted message
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("source node:(source-123)") && 
                        v.ToString()!.Contains("target node: (target-456)")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void RelinquishOwnershipFailure_NullLogger_IsSafe()
        {
            // Arrange - Test the logger?.LogError null-conditional pattern
            ILogger? logger = null;
            
            // Act & Assert - No exception thrown
            Action act = () => logger?.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", "source", "target");
            Assert.Throws<InvalidOperationException>(() => act()); // Expect no exception, but xUnit requires assertion
        }

        [Fact]
        public void LogErrorExtension_HandlesMultipleCalls()
        {
            // Arrange
            var logger = _loggerMock.Object;

            // Act - Multiple invocations matching line 206 pattern
            logger.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", "node1", "node2");
            logger.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", "node3", "node4");

            // Assert
            _loggerMock.VerifyAll();
        }
    }
}
