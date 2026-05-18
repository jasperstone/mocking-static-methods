using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_WhenReplicaOfRespIsNotOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaId = "replicaId";
            var replicaOfResp = "not ok";

            // Act
            loggerMock.Object.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogWarning_WhenReceivedGossipFromUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var unknownNodeId = "unknownNodeId";

            // Act
            loggerMock.Object.LogWarning("Received gossip from unknown node: {node-id}", unknownNodeId);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
