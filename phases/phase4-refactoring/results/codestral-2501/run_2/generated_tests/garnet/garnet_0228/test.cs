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
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions();

            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("username");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("password");

            replicationManagerMock.Setup(rm => rm.storeWrapper).Returns(storeWrapperMock.Object);
            replicationManagerMock.Setup(rm => rm.logger).Returns(loggerMock.Object);

            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig());

            var replicaReceiveCheckpoint = new ReplicationManagerWrapper(clusterProviderMock.Object);

            // Act
            var result = await replicaReceiveCheckpoint.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

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

    public class ReplicationManagerWrapper
    {
        private readonly ReplicationManager _replicationManager;

        public ReplicationManagerWrapper(ClusterProvider clusterProvider)
        {
            _replicationManager = new ReplicationManager(clusterProvider);
        }

        public Task<(bool Success, ReadOnlyMemory<byte> ErrorMessage)> TryReplicateDiskbasedSyncAsync(ClusterSession session, ReplicateSyncOptions options)
        {
            return _replicationManager.TryReplicateDiskbasedSyncAsync(session, options);
        }
    }
}
