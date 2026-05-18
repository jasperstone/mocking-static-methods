using Moq;
using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsError_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

            // Act
            try
            {
                await replicationManager.TryReplicateDiskbasedSyncAsync(null, new ReplicateSyncOptions());
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ReplicaSyncAttachTaskAsync_LogsError_OnNullAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig.GetLocalNodePrimaryAddress()).Returns((null, -1));

            // Act
            var result = await replicationManager.ReplicaSyncAttachTaskAsync(false, false);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
