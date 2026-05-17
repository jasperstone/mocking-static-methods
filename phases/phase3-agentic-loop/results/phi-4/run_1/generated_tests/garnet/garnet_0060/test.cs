using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarningIsCalled_WhenGossipFromUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clientMock = new Mock<IGarnetClient>();
            var cts = new CancellationTokenSource();
            var failoverTimeout = TimeSpan.FromSeconds(5);
            var oldConfig = new ClusterConfig(); // Assuming a default constructor or a suitable setup
            var replicaId = "unknown-node-id";
            var replicaOfResp = "unexpected-response";

            var session = new ReplicaFailoverSession(
                clusterProviderMock.Object,
                loggerMock.Object,
                oldConfig,
                failoverTimeout,
                cts.Token);

            // Mock the necessary methods and properties
            clusterProviderMock.Setup(cp => cp.clusterManager.gossipStats.UpdateGossipBytesRecv(It.IsAny<int>()));
            clusterProviderMock.Setup(cp => cp.clusterManager.TryMerge(It.IsAny<ClusterConfig>())).Returns(false);
            clientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                      .ReturnsAsync(replicaOfResp);

            // Act
            await session.BroadcastConfigAndRequestAttachAsync(replicaId, new byte[0]);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s.Contains("Received gossip from unknown node: {node-id}")),
                    It.Is<object[]>(o => o[0].ToString() == replicaId)),
                Times.Once);
        }
    }
}
