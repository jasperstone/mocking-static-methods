using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using System;

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

        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);

        var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);

        var options = new ReplicateSyncOptions
        {
            Background = false,
            NodeId = "node1",
            TryAddReplica = true,
            Force = true,
            UpgradeLock = true
        };

        // Act
        await replicationManager.TryReplicateDiskbasedSyncAsync(mockSession.Object, options);

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation("Initiating foreground checkpoint retrieval"),
            Times.Once);
    }
}
