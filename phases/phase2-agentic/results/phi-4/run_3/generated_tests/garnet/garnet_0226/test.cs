using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundCheckpointRetrieval()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockSession = new Mock<ClusterSession>();
            var mockOptions = new ReplicateSyncOptions
            {
                Background = false,
                TryAddReplica = false,
                NodeId = "testNodeId",
                Force = false,
                UpgradeLock = false
            };

            mockClusterProvider.Setup(p => p.clusterManager).Returns(mockClusterManager.Object);
            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(mockSession.Object, mockOptions);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s == "Initiating foreground checkpoint retrieval"),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
