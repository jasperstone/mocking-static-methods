using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

public class ReplicaReceiveCheckpointTests
{
    [Fact]
    public async Task TryReplicateDiskbasedSyncAsync_ShouldLogError_WhenPrimaryAddressIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var clusterManagerMock = new Mock<ClusterManager>();
        var replicationManagerMock = new Mock<ReplicationManager>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var serverOptionsMock = new Mock<ServerOptions>();
        var currentConfigMock = new Mock<ClusterConfig>();

        clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
        clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
        clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptionsMock.Object);
        clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);

        clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);
        currentConfigMock.Setup(cc => cc.GetLocalNodePrimaryAddress()).Returns((null, -1));

        var replicaReceiveCheckpoint = new ReplicationManager
        {
            logger = loggerMock.Object,
            clusterProvider = clusterProviderMock.Object
        };

        // Act
        var result = await replicaReceiveCheckpoint.TryReplicateDiskbasedSyncAsync(null, new ReplicateSyncOptions());

        // Assert
        loggerMock.Verify(
            l => l.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
