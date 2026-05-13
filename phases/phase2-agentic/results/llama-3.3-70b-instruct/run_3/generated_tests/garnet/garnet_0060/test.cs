using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicasAsync_LogsWarning_WhenReplicaOfRespIsNotOK()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<GarnetClient>();
            var replicaFailoverSession = new ReplicaFailoverSession(loggerMock.Object, clientMock.Object);

            clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("NOT_OK");

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", It.IsAny<string>(), "NOT_OK"), Times.Once);
        }

        [Fact]
        public async Task IssueAttachReplicasAsync_LogsWarning_WhenReceivedGossipFromUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<GarnetClient>();
            var replicaFailoverSession = new ReplicaFailoverSession(loggerMock.Object, clientMock.Object);

            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new byte[] { });

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Received gossip from unknown node: {node-id}", It.IsAny<string>()), Times.Once);
        }
    }
}
