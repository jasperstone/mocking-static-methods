using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Garnet.server;

namespace Garnet.cluster.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsError_WhenPrimaryAddressIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            var serverOptionsMock = new Mock<IServerOptions>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions { NodeId = 1, TryAddReplica = true, Force = true, UpgradeLock = true, Background = false };

            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptionsMock.Object);
            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig());
            replicationManagerMock.Setup(rm => rm.GetIRSNetworkBufferSettings).Returns(new IRSNetworkBufferSettings());
            replicationManagerMock.Setup(rm => rm.GetNetworkPool).Returns(new NetworkPool());
            storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            serverOptionsMock.Setup(so => so.EnableFastCommit).Returns(false);

            var replicationManager = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object, storeWrapperMock.Object);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

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
