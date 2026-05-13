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
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReceivedGossipFromUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clientMock = new Mock<GarnetClient>();
            var oldConfig = new ClusterConfig { LocalNodePrimaryId = "primaryId" };
            var newConfig = new ClusterConfig();
            var replicaId = "unknownReplicaId";
            var configByteArray = new byte[0];
            var failoverTimeout = TimeSpan.FromSeconds(5);
            var cts = new CancellationTokenSource();

            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig).Returns(newConfig);
            clusterProviderMock.Setup(c => c.clusterManager.TryMerge(It.IsAny<ClusterConfig>())).Returns(false);
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig.GetReplicaIds(It.IsAny<string>())).Returns(new[] { replicaId });

            var session = new ReplicaFailoverSession(clusterProviderMock.Object, loggerMock.Object, oldConfig, newConfig, failoverTimeout);

            // Act
            await session.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("Received gossip from unknown node:")), It.Is<string>(s => s == replicaId)),
                Times.Once);
        }
    }
}
