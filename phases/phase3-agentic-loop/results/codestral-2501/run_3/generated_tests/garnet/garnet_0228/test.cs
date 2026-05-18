using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Garnet.server;
using System.Net;

public class ReplicaReceiveCheckpointTests
{
    [Fact]
    public async Task TryReplicateDiskbasedSyncAsync_ShouldLogError_WhenAddressIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicationManager>>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var clusterManagerMock = new Mock<ClusterManager>();
        var replicationManagerMock = new Mock<ReplicationManager>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var appendOnlyFileMock = new Mock<AppendOnlyFile>();

        clusterProviderMock.Setup(cp => cp.ClusterManager).Returns(clusterManagerMock.Object);
        clusterProviderMock.Setup(cp => cp.ReplicationManager).Returns(replicationManagerMock.Object);
        clusterProviderMock.Setup(cp => cp.ServerOptions).Returns(new ServerOptions());
        clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("username");
        clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("password");

        clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig());
        replicationManagerMock.Setup(rm => rm.GetIRSNetworkBufferSettings).Returns(new IRSNetworkBufferSettings());
        replicationManagerMock.Setup(rm => rm.GetNetworkPool).Returns(new NetworkPool());
        storeWrapperMock.Setup(sw => sw.AppendOnlyFile).Returns(appendOnlyFileMock.Object);

        var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object, storeWrapperMock.Object);

        // Act
        var result = await replicationManager.TryReplicateDiskbasedSyncAsync(null, new ReplicateSyncOptions());

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
