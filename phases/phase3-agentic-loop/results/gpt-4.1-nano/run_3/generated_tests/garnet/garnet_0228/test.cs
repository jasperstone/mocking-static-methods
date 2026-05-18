using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogError_When_AddressIsNullOrPortIsMinusOne()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockReplicationManager = new Mock<IReplicationManager>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockSession = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions
            {
                NodeId = "node1",
                TryAddReplica = true,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            var mockConfig = new Mock<IClusterConfig>();
            mockConfig.Setup(c => c.GetLocalNodePrimaryAddress()).Returns((string)null, -1);
            mockClusterManager.Setup(c => c.CurrentConfig).Returns(mockConfig.Object);
            mockClusterProvider.Setup(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(c => c.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(c => c.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(c => c.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(c => c.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(c => c.logger).Returns(mockLogger.Object);
            mockClusterProvider.Setup(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(c => c.IsRecovering).Returns(true);
            mockClusterProvider.Setup(c => c.ResetReplayIterator()).Verifiable();

            var replicationManager = new ReplicationManager
            {
                logger = mockLogger.Object,
                clusterProvider = mockClusterProvider.Object,
                storeWrapper = mockStoreWrapper.Object,
                cEntry = null
            };

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(mockSession.Object, options);

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("TryReplicateDiskbasedSyncAsync"))),
                Times.Once);
            Assert.False(result.Success);
        }
    }
}
