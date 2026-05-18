using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class MigrationLoggerTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public MigrationLoggerTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogError_RelinquishOwnershipFailure_LogsCorrectMessage()
        {
            // Arrange
            string sourceNodeId = "source-node-1";
            string targetNodeId = "target-node-1";

            // Act
            _loggerMock.Object.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", sourceNodeId, targetNodeId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to relinquish ownership from source node:(source-node-1) to target node: (target-node-1)")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_MigrateSlotsDriverFailure_LogsCorrectMessage()
        {
            // Arrange
            string expectedMessage = "MigrateSlotsDriver failed";

            // Act
            _loggerMock.Object.LogError(expectedMessage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
