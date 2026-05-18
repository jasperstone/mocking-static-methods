using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_WhenReceivedGossipFromUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var oldConfigMock = new Mock<ClusterConfig>();
            var clientMock = new Mock<GarnetClient>();
            var failoverSession = new ReplicaFailoverSession(
                clusterProviderMock.Object,
                oldConfigMock.Object,
                loggerMock.Object);

            // Setup mocks
            var unknownNodeId = "unknown-node-id";
            oldConfigMock.Setup(c => c.IsKnown(unknownNodeId)).Returns(false);

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync(unknownNodeId, new byte[0]);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s == "Received gossip from unknown node: {node-id}"),
                    It.Is<object[]>(o => o[0].ToString() == unknownNodeId)),
                Times.Once);
        }
    }
}
