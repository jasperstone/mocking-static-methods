using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private readonly Mock<ILogger<ReplicaFailoverSession>> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly ReplicaFailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _storeWrapperMock = new Mock<StoreWrapper>();

            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);

            _session = new ReplicaFailoverSession(_loggerMock.Object, _clusterProviderMock.Object);
        }

        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_LogsErrorAndReturnsFalse_WhenClientIsNull()
        {
            // Arrange
            _session.oldConfig = new Mock<IOldConfig>().Object;
            _session.oldConfig.LocalNodePrimaryId = "primaryId";
            _session.oldConfig.LocalNodeId = "localId";

            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ReturnsAsync(0L);
            _session.GetType().GetField("primaryClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_session, null);
            _session.GetType().GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_session, _loggerMock.Object);
            _session.GetType().GetField("clusterProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_session, _clusterProviderMock.Object);
            _session.GetType().GetField("failoverTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_session, TimeSpan.FromSeconds(1));
            _session.GetType().GetField("cts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_session, new System.Threading.CancellationTokenSource());

            // Act
            var result = await _session.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarningAndReturnsFalse_WhenCannotBeginRecovery()
        {
            // Arrange
            _clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(false);
            _clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.replicationManager.TryUpdateForFailover());
            _clusterProviderMock.Setup(cp => cp.replicationManager.ResetReplayIterator());
            _clusterProviderMock.Setup(cp => cp.replicationManager.InitializeCheckpointStore()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            _clusterProviderMock.Setup(cp => cp.storeWrapper.StartPrimaryTasks());

            // Act
            var result = await _session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("TakeOverAsPrimaryAsync")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarningAndReturnsFalse_WhenTryTakeOverFails()
        {
            // Arrange
            _clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(true);
            _clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(false);

            // Act
            var result = await _session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("TakeOverAsPrimaryAsync")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_DisablesRecovery_WhenSucceeded()
        {
            // Arrange
            _clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(true);
            _clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.replicationManager.TryUpdateForFailover());
            _clusterProviderMock.Setup(cp => cp.replicationManager.ResetReplayIterator());
            _clusterProviderMock.Setup(cp => cp.replicationManager.InitializeCheckpointStore()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            _clusterProviderMock.Setup(cp => cp.storeWrapper.StartPrimaryTasks());
            _clusterProviderMock.Setup(cp => cp.replicationManager.EndRecovery(RecoveryStatus.NoRecovery, false));

            // Act
            var result = await _session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.True(result);
            _clusterProviderMock.Verify(cp => cp.replicationManager.EndRecovery(RecoveryStatus.NoRecovery, false), Times.Once);
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsError_WhenClientIsNull()
        {
            // Arrange
            var replicaId = "replica1";
            var configData = new byte[] { 1, 2, 3 };
            _session.oldConfig = new Mock<IOldConfig>().Object;
            _session.oldConfig.LocalNodePrimaryId = "primaryId";
            _session.oldConfig.LocalNodeId = "localId";

            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new Mock<IClusterConfig>().Object);
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.GetConnectionAsync(It.IsAny<string>())).ReturnsAsync((GarnetClient)null);

            // Act
            await _session.BroadcastConfigAndRequestAttachAsync(replicaId, configData);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
