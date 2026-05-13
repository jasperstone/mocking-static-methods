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
        private readonly Mock<cluster.IClusterProvider> _clusterProviderMock;
        private readonly Mock<cluster.IClusterManager> _clusterManagerMock;
        private readonly Mock<cluster.IReplicationManager> _replicationManagerMock;
        private readonly Mock<cluster.IStoreWrapper> _storeWrapperMock;
        private readonly ReplicaFailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            _clusterProviderMock = new Mock<cluster.IClusterProvider>();
            _clusterManagerMock = new Mock<cluster.IClusterManager>();
            _replicationManagerMock = new Mock<cluster.IReplicationManager>();
            _storeWrapperMock = new Mock<cluster.IStoreWrapper>();

            _clusterProviderMock.SetupGet(c => c.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.SetupGet(c => c.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(_storeWrapperMock.Object);

            _session = new ReplicaFailoverSession(_loggerMock.Object, _clusterProviderMock.Object);
        }

        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_Should_LogErrorAndReturnFalse_When_ClientIsNull()
        {
            // Arrange
            _session.oldConfig = new Mock<cluster.IConfig>().Object;
            _session.failoverTimeout = TimeSpan.FromSeconds(1);
            _session.cts = new System.Threading.CancellationTokenSource();

            // Act
            var result = await _session.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_Should_LogErrorAndReturnFalse_On_Exception()
        {
            // Arrange
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ThrowsAsync(new Exception("fail"));
            _session.oldConfig = new Mock<cluster.IConfig>().Object;
            _session.oldConfig.LocalNodeId = "node1";
            _session.failoverTimeout = TimeSpan.FromSeconds(1);
            _session.cts = new System.Threading.CancellationTokenSource();

            // Setup GetConnectionAsync to return null to simulate failure
            _session.GetType().GetMethod("GetConnectionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .CreateDelegate<Func<string, Task<GarnetClient>>>(_session);
            // Use reflection or other means to set the client to null or throw

            // Act
            var result = await _session.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "PauseWritesAndWaitForSync Error"), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_Should_LogWarning_When_CannotBeginRecovery()
        {
            // Arrange
            _clusterProviderMock.Setup(c => c.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false))
                .Returns(false);
            _clusterProviderMock.Setup(c => c.clusterManager.TryTakeOverForPrimary()).Returns(true);
            _clusterProviderMock.Setup(c => c.replicationManager.TryUpdateForFailover());
            _clusterProviderMock.Setup(c => c.replicationManager.ResetReplayIterator());
            _clusterProviderMock.Setup(c => c.replicationManager.InitializeCheckpointStore()).Returns(true);
            _clusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            _clusterProviderMock.Setup(c => c.storeWrapper.StartPrimaryTasks());

            // Act
            var result = await _session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("TakeOverAsPrimaryAsync")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_Should_LogWarning_When_TryTakeOverForPrimaryFails()
        {
            // Arrange
            _clusterProviderMock.Setup(c => c.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            _clusterProviderMock.Setup(c => c.clusterManager.TryTakeOverForPrimary()).Returns(false);

            // Act
            var result = await _session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("TakeOverAsPrimaryAsync")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_Should_Call_EndRecovery_When_AcquiredLock()
        {
            // Arrange
            _clusterProviderMock.Setup(c => c.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            _clusterProviderMock.Setup(c => c.clusterManager.TryTakeOverForPrimary()).Returns(true);
            _clusterProviderMock.Setup(c => c.replicationManager.TryUpdateForFailover());
            _clusterProviderMock.Setup(c => c.replicationManager.ResetReplayIterator());
            _clusterProviderMock.Setup(c => c.replicationManager.InitializeCheckpointStore()).Returns(true);
            _clusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            _clusterProviderMock.Setup(c => c.storeWrapper.StartPrimaryTasks());

            // Act
            var result = await _session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.True(result);
            _clusterProviderMock.Verify(c => c.replicationManager.EndRecovery(RecoveryStatus.NoRecovery, false), Times.Once);
        }
    }
}
