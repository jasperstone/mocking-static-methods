using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        private Mock<ClusterProvider> mockClusterProvider;
        private Mock<ILogger> mockLogger;
        private ReplicationManager replicationManager;

        public ReplicationManagerTests()
        {
            mockClusterProvider = new Mock<ClusterProvider>();
            mockLogger = new Mock<ILogger>();
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new ClusterManager());
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(new StoreWrapper());
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(new GarnetClusterCheckpointManager());
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.PRIMARY });
            replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
        }

        [Fact]
        public void BeginRecovery_ShouldReturnFalseAndLogError_WhenCurrentRecoveryStatusIsNotNoRecoveryOrReadRole()
        {
            // Arrange
            replicationManager.currentRecoveryStatus = RecoveryStatus.Recovering;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<RecoveryStatus>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_ShouldReturnFalseAndLogError_WhenTryUpgradeReadLockFails()
        {
            // Arrange
            var mockLock = new Mock<SingleWriterMultiReaderLock>();
            mockLock.Setup(l => l.TryUpgradeReadLock()).Returns(false);
            replicationManager.recoverLock = mockLock.Object;
            replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_ShouldReturnTrueAndLogTrace_WhenUpgradeLockSucceeds()
        {
            // Arrange
            var mockLock = new Mock<SingleWriterMultiReaderLock>();
            mockLock.Setup(l => l.TryUpgradeReadLock()).Returns(true);
            replicationManager.recoverLock = mockLock.Object;
            replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, true);

            // Assert
            Assert.True(result);
            mockLogger.Verify(
                x => x.LogTrace(It.IsAny<string>(), It.IsAny<RecoveryStatus>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_ShouldReturnFalseAndLogError_WhenCurrentRecoveryStatusIsNotNoRecoveryAndNotReadRole()
        {
            // Arrange
            replicationManager.currentRecoveryStatus = RecoveryStatus.Recovering;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<RecoveryStatus>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_ShouldReturnFalseAndLogError_WhenTryPauseCheckpointsFails()
        {
            // Arrange
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new ClusterManager());
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(new GarnetClusterCheckpointManager());
            var mgr = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            mgr.currentRecoveryStatus = RecoveryStatus.NoRecovery;

            var mockStore = new Mock<Store>();
            mockStore.Setup(s => s.CheckpointManager).Returns(new CheckpointManager());
            mockStoreWrapper.Setup(s => s.store).Returns(mockStore.Object);
            mockStoreWrapper.Setup(s => s.TryPauseCheckpoints()).Returns(false);

            // Act
            var result = mgr.BeginRecovery(RecoveryStatus.CheckpointRecoveredAtPrimary, false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<RecoveryStatus>()),
                Times.Once);
        }
    }
}
