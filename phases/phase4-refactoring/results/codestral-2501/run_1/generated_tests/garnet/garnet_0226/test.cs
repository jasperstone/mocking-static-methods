using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Garnet.server;

public class ReplicaReceiveCheckpointTests
{
    [Fact]
    public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundCheckpointRetrieval()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ReplicationManager>>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockClusterManager = new Mock<ClusterManager>();
        var mockSession = new Mock<ClusterSession>();
        var options = new ReplicateSyncOptions
        {
            Background = false,
            UpgradeLock = false,
            NodeId = "node1",
            TryAddReplica = false
        };

        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterManager.Setup(cm => cm.TryAddReplicaAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
            .ReturnsAsync((true, default(ReadOnlyMemory<byte>)));

        var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);

        // Act
        await replicationManager.TryReplicateDiskbasedSyncAsync(mockSession.Object, options);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation("Initiating foreground checkpoint retrieval"),
            Times.Once);
    }
}
