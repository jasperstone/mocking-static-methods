using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void TryReplicateDiskbasedSyncAsync_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager(loggerMock.Object);
            var session = new ClusterSession();
            var options = new ReplicateSyncOptions();

            // Act and Assert
            var result = replicationManager.TryReplicateDiskbasedSyncAsync(session, options);
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ReplicaSyncAttachTaskAsync_LogsError_WhenNoPrimaryAssigned()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager(loggerMock.Object);
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig.GetLocalNodePrimaryAddress()).Returns((null, -1));
            replicationManager.clusterProvider = clusterProviderMock.Object;

            // Act and Assert
            var result = replicationManager.ReplicaSyncAttachTaskAsync(false, false);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
