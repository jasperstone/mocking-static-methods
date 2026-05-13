using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Garnet.client;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsCheckpointSearchCompleted()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockCurrentConfig = new Mock<CurrentConfig>();
            var mockGarnetClientSession = new Mock<GarnetClientSession>();

            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(new Mock<ReplicationLogCheckpointManager>().Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(new Mock<ReplicationLogCheckpointManager>().Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockCurrentConfig.Object);
            mockCurrentConfig.Setup(cc => cc.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns((IPAddress.Parse("127.0.0.1"), 12345));
            mockReplicationManager.Setup(rm => rm.GetRSSNetworkBufferSettings).Returns(new RSSNetworkBufferSettings());
            mockReplicationManager.Setup(rm => rm.GetNetworkPool).Returns(new NetworkPool());
            mockServerOptions.Setup(so => so.TlsOptions).Returns(new TlsOptions());
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(new ServerOptions { ReplicaSyncTimeout = TimeSpan.FromSeconds(10) });

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaNodeId: "replicaNodeId",
                replicaCheckpointEntry: new CheckpointEntry { metadata = new CheckpointMetadata { storeVersion = 1, objectStoreVersion = 1 } },
                logger: mockLogger.Object
            );

            // Act
            var result = await replicaSyncSession.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    "Checkpoint search completed",
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }
    }
}
