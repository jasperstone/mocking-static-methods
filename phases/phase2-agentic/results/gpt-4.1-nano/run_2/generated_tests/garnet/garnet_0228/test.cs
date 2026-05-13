using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Garnet.cluster;
using Garnet.server;
using Garnet.client;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        private readonly Mock<ILogger<ReplicationManager>> _loggerMock;
        private readonly Mock<IClusterProvider> _clusterProviderMock;
        private readonly Mock<IClusterManager> _clusterManagerMock;
        private readonly Mock<IReplicationManager> _replicationManagerMock;
        private readonly Mock<IStoreWrapper> _storeWrapperMock;
        private readonly Mock<ClusterSession> _sessionMock;
        private readonly ReplicationManager _replicationManager;

        public ReplicaReceiveCheckpointTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _clusterManagerMock = new Mock<IClusterManager>();
            _replicationManagerMock = new Mock<IReplicationManager>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            _sessionMock = new Mock<ClusterSession>();

            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _replicationManager = new ReplicationManager
            {
                logger = _loggerMock.Object,
                clusterProvider = _clusterProviderMock.Object,
                storeWrapper = _storeWrapperMock.Object,
                ctsRepManager = new CancellationTokenSource()
            };
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsErrorOnException()
        {
            // Arrange
            var options = new ReplicateSyncOptions { NodeId = 1, TryAddReplica = false, Background = false, Force = false, UpgradeLock = false };
            _clusterManagerMock.Setup(cm => cm.TryAddReplicaAsync(It.IsAny<int>(), false, false, null))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _replicationManager.TryReplicateDiskbasedSyncAsync(_sessionMock.Object, options);

            // Assert
            Assert.False(result.Success);
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(ReplicationManager.TryReplicateDiskbasedSyncAsync)))),
                Times.Once);
        }

        [Fact]
        public async Task ReplicaSyncAttachTaskAsync_LogsErrorWhenNoPrimaryAddress()
        {
            // Arrange
            var options = new ReplicateSyncOptions { NodeId = 1, TryAddReplica = false, Background = false, Force = false, UpgradeLock = false };
            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig { LocalNodeId = 1 });
            _clusterManagerMock.Setup(cm => cm.CurrentConfig.GetLocalNodePrimaryAddress())
                .Returns((null, -1));

            var method = typeof(ReplicationManager).GetMethod("TryReplicateDiskbasedSyncAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<(bool, ReadOnlyMemory<byte>)>)method.Invoke(_replicationManager, new object[] { _sessionMock.Object, options });

            // Act
            var result = await task;

            // Assert
            Assert.False(result.Item1);
            Assert.Contains("RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR", Encoding.ASCII.GetString(result.Item2.ToArray()));
            _loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
