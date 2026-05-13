using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;
using Garnet.server;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        private readonly Mock<ILogger<ReplicationManager>> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<ClusterSession> _sessionMock;
        private readonly Mock<ILogger> _logger;
        private readonly ReplicationManager _replicationManager;

        public ReplicaReceiveCheckpointTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _sessionMock = new Mock<ClusterSession>();
            _logger = new Mock<ILogger>();

            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.GetLatestCheckpointEntryFromDisk()).Returns(new CheckpointEntry());

            _replicationManager = new ReplicationManager
            {
                logger = _loggerMock.Object,
                clusterProvider = _clusterProviderMock.Object,
                storeWrapper = _storeWrapperMock.Object,
                cEntry = new CheckpointEntry()
            };
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var options = new ReplicateSyncOptions { NodeId = 1, TryAddReplica = false, Background = false, Force = false, UpgradeLock = false };
            var session = _sessionMock.Object;

            _clusterManagerMock.Setup(cm => cm.TryAddReplicaAsync(It.IsAny<int>(), false, false, null))
                .ReturnsAsync((true, (string)null));

            _sessionMock.Setup(s => s.UnsafeBumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            _loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>())).Verifiable();

            // Act
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _replicationManager.TryReplicateDiskbasedSyncAsync(session, options);
            });

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), $"{nameof(_replicationManager.TryReplicateDiskbasedSyncAsync)}"), Times.Once);
        }

        [Fact]
        public async Task ReplicaSyncAttachTaskAsync_Should_LogError_When_AddressIsNull()
        {
            // Arrange
            var options = new ReplicateSyncOptions { NodeId = 1, TryAddReplica = false, Background = false, Force = false, UpgradeLock = false };
            var session = _sessionMock.Object;

            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig());
            _clusterManagerMock.Setup(cm => cm.CurrentConfig.GetLocalNodePrimaryAddress()).Returns((null, -1));

            var method = typeof(ReplicationManager).GetMethod("TryReplicateDiskbasedSyncAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<(bool, ReadOnlyMemory<byte>)>)method.Invoke(_replicationManager, new object[] { session, options });

            // Act
            var result = await task;

            // Assert
            Assert.False(result.Item1);
            Assert.Contains("RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR", Encoding.ASCII.GetString(result.Item2.ToArray()));
        }
    }
}
