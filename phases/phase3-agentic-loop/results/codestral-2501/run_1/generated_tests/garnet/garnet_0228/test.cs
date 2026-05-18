using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Garnet.server;

public class ReplicaReceiveCheckpointTests
{
    [Fact]
    public async Task TryReplicateDiskbasedSyncAsync_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicationManager>>();
        var clusterProviderMock = new Mock<IClusterProvider>();
        var clusterManagerMock = new Mock<IClusterManager>();
        var replicationManagerMock = new Mock<IReplicationManager>();
        var sessionMock = new Mock<ClusterSession>();
        var options = new ReplicateSyncOptions();

        var replicationManager = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object, replicationManagerMock.Object);

        // Act
        await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains(nameof(replicationManager.TryReplicateDiskbasedSyncAsync))),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ReplicaSyncAttachTaskAsync_LogsError_WhenAddressIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicationManager>>();
        var clusterProviderMock = new Mock<IClusterProvider>();
        var clusterManagerMock = new Mock<IClusterManager>();
        var replicationManagerMock = new Mock<IReplicationManager>();
        var currentConfigMock = new Mock<ClusterConfig>();
        var replicationManager = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object, replicationManagerMock.Object);

        clusterProviderMock.Setup(x => x.clusterManager).Returns(clusterManagerMock.Object);
        clusterManagerMock.Setup(x => x.CurrentConfig).Returns(currentConfigMock.Object);
        currentConfigMock.Setup(x => x.GetLocalNodePrimaryAddress()).Returns((null, -1));

        // Act
        var result = await replicationManager.ReplicaSyncAttachTaskAsync(false, false);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("{msg}")),
                It.Is<string>(s => s.Contains("not assigned primary error")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}
