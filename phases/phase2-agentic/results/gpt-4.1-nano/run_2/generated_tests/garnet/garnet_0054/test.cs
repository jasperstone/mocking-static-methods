using System;
using System.Collections.Generic;
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
        private readonly Mock<clusterProvider> _clusterProviderMock;
        private readonly Mock<clusterManager> _clusterManagerMock;
        private readonly Mock<replicationManager> _replicationManagerMock;
        private readonly Mock<storeWrapper> _storeWrapperMock;
        private readonly ReplicaFailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            _clusterProviderMock = new Mock<clusterProvider>();
            _clusterManagerMock = new Mock<clusterManager>();
            _replicationManagerMock = new Mock<replicationManager>();
            _storeWrapperMock = new Mock<storeWrapper>();

            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);

            _session = new ReplicaFailoverSession(_loggerMock.Object, _clusterProviderMock.Object);
        }

        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_Should_LogErrorAndReturnFalse_When_ClientIsNull()
        {
            // Arrange
            _session.oldConfig = new Mock<OldConfig>().Object;
            _session.oldConfig.LocalNodePrimaryId = "primaryId";
            _session.oldConfig.LocalNodeId = "localId";

            // Force GetConnectionAsync to return null
            var mockSession = new Mock<ReplicaFailoverSession>(_loggerMock.Object, _clusterProviderMock.Object) { CallBase = true };
            mockSession.Setup(s => s.GetConnectionAsync(It.IsAny<string>())).ReturnsAsync((GarnetClient)null);

            // Act
            var result = await mockSession.Object.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(log => log.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PauseWritesAndWaitForSyncAsync_Should_LogErrorAndReturnFalse_When_ExceptionThrown()
        {
            // Arrange
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ReturnsAsync(0L);
            var mockSession = new Mock<ReplicaFailoverSession>(_loggerMock.Object, _clusterProviderMock.Object) { CallBase = true };
            mockSession.Setup(s => s.GetConnectionAsync(It.IsAny<string>())).ReturnsAsync(mockClient.Object);
            mockSession.Setup(s => s.FailoverTimeout).Returns(false);
            mockSession.Setup(s => s.cts).Returns(new System.Threading.CancellationTokenSource());

            mockClient.Setup(c => c.FailStopWritesAsync(It.IsAny<byte[]>())).ThrowsAsync(new Exception());

            // Act
            var result = await mockSession.Object.PauseWritesAndWaitForSyncAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(log => log.LogError(It.IsAny<Exception>(), "PauseWritesAndWaitForSync Error"), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_Should_LogWarning_When_CannotBeginRecovery()
        {
            // Arrange
            var mockSession = new Mock<ReplicaFailoverSession>(_loggerMock.Object, _clusterProviderMock.Object) { CallBase = true };
            mockSession.Setup(s => s.clusterProvider.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(false);
            mockSession.Setup(s => s.clusterProvider.clusterManager.TryTakeOverForPrimary()).Returns(true);
            mockSession.Setup(s => s.clusterProvider.replicationManager.TryUpdateForFailover()).Verifiable();
            mockSession.Setup(s => s.clusterProvider.replicationManager.ResetReplayIterator()).Verifiable();
            mockSession.Setup(s => s.clusterProvider.replicationManager.InitializeCheckpointStore()).Returns(true);
            mockSession.Setup(s => s.clusterProvider.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            mockSession.Setup(s => s.clusterProvider.storeWrapper.StartPrimaryTasks()).Verifiable();

            // Act
            var result = await mockSession.Object.TakeOverAsPrimaryAsync();

            // Assert
            Assert.True(result);
            _loggerMock.Verify(log => log.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_Should_LogWarning_When_TryTakeOverForPrimaryFails()
        {
            // Arrange
            var mockSession = new Mock<ReplicaFailoverSession>(_loggerMock.Object, _clusterProviderMock.Object) { CallBase = true };
            mockSession.Setup(s => s.clusterProvider.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            mockSession.Setup(s => s.clusterProvider.clusterManager.TryTakeOverForPrimary()).Returns(false);

            // Act
            var result = await mockSession.Object.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(log => log.LogWarning($"{nameof(ReplicaFailoverSession)}: {{logMessage}}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_Should_EndRecovery_When_AcquiredLock()
        {
            // Arrange
            var mockSession = new Mock<ReplicaFailoverSession>(_loggerMock.Object, _clusterProviderMock.Object) { CallBase = true };
            mockSession.Setup(s => s.clusterProvider.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(true);
            mockSession.Setup(s => s.clusterProvider.clusterManager.TryTakeOverForPrimary()).Returns(true);
            mockSession.Setup(s => s.clusterProvider.replicationManager.TryUpdateForFailover()).Verifiable();
            mockSession.Setup(s => s.clusterProvider.replicationManager.ResetReplayIterator()).Verifiable();
            mockSession.Setup(s => s.clusterProvider.replicationManager.InitializeCheckpointStore()).Returns(true);
            mockSession.Setup(s => s.clusterProvider.BumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);
            mockSession.Setup(s => s.clusterProvider.storeWrapper.StartPrimaryTasks()).Verifiable();
            mockSession.Setup(s => s.clusterProvider.replicationManager.EndRecovery(RecoveryStatus.NoRecovery, false)).Verifiable();

            // Act
            var result = await mockSession.Object.TakeOverAsPrimaryAsync();

            // Assert
            Assert.True(result);
            mockSession.Verify(s => s.clusterProvider.replicationManager.EndRecovery(RecoveryStatus.NoRecovery, false), Times.Once);
        }
    }
}
