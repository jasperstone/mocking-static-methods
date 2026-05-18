using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Garnet.common;
using Garnet.server;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsCheckpointSearchCompleted()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockReplicaSyncMetadata = new Mock<SyncMetadata>();
            var mockReplicaCheckpointEntry = new Mock<CheckpointEntry>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockServerOptions = new Mock<ServerOptions>();

            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(new Mock<IReplicationLogCheckpointManager>().Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(new Mock<IReplicationLogCheckpointManager>().Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(new Mock<ClusterConfig>().Object);
            mockReplicationManager.Setup(rm => rm.GetRSSNetworkBufferSettings).Returns(new Mock<NetworkBufferSettings>().Object);
            mockReplicationManager.Setup(rm => rm.GetNetworkPool).Returns(new Mock<NetworkPool>().Object);
            mockServerOptions.Setup(so => so.TlsOptions).Returns(new Mock<TlsOptions>().Object);
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("username");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("password");

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                mockReplicaSyncMetadata.Object,
                CancellationToken.None,
                "replicaNodeId",
                "replicaAssignedPrimaryId",
                mockReplicaCheckpointEntry.Object,
                0,
                0,
                mockLogger.Object);

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Checkpoint search completed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
