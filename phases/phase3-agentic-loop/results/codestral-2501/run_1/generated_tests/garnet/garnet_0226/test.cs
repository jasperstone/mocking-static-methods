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
        var options = new ReplicateSyncOptions
        {
            Background = false,
            UpgradeLock = false,
            NodeId = "node1",
            TryAddReplica = true
        };

        mockClusterProvider.Setup(cp => cp.ClusterManager).Returns(mockClusterManager.Object);
        mockClusterManager.Setup(cm => cm.TryAddReplicaAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
            .ReturnsAsync((true, default(ReadOnlyMemory<byte>)));

        var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);

        // Act
        await replicationManager.TryReplicateDiskbasedSyncAsync(mockSession.Object, options);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Initiating foreground checkpoint retrieval")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
