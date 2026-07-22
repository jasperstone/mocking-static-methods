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

namespace Garnet.cluster.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsError_WhenAddressIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var currentConfigMock = new Mock<CurrentConfig>();

            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("username");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("password");

            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);
            currentConfigMock.Setup(cc => cc.GetLocalNodePrimaryAddress()).Returns((null, -1));

            var replicaReceiveCheckpoint = new ReplicaReceiveCheckpoint(clusterProviderMock.Object, loggerMock.Object, storeWrapperMock.Object);

            // Act
            var result = await replicaReceiveCheckpoint.TryReplicateDiskbasedSyncAsync(null, new ReplicateSyncOptions());

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
}
