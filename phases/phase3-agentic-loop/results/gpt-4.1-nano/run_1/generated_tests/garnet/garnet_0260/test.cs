using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;

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
            // Setup minimal required properties and methods for constructor
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<Store>();
            var mockObjectStore = new Mock<Store>();
            var mockCheckpointManager = new Mock<GarnetClusterCheckpointManager>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockConfig = new Config { LocalNodeRole = NodeRole.REPLICA, Recover = true };
            mockClusterManager.Setup(c => c.CurrentConfig).Returns(mockConfig);
            mockClusterProvider.Setup(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(c => c.serverOptions).Returns(mockServerOptions.Object);
            mockStoreWrapper.Setup(s => s.store).Returns(mockStore.Object);
            mockStoreWrapper.Setup(s => s.objectStore).Returns(mockObjectStore.Object);
            mockClusterProvider.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockCheckpointManager.Object);
            // Instantiate ReplicationManager
            replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
        }

        [Fact]
        public void BeginRecovery_UpgradeLock_Success()
        {
            // Arrange
            var initialStatus = RecoveryStatus.NoRecovery;
            var nextStatus = RecoveryStatus.ReadRole;
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();
            mockRecoverLock.Setup(r => r.TryUpgradeReadLock()).Returns(true);
            // Inject mock recoverLock
            var recoverLockField = typeof(ReplicationManager).GetField("recoverLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            recoverLockField.SetValue(replicationManager, mockRecoverLock.Object);

            // Act
            var result = replicationManager.BeginRecovery(nextStatus, upgradeLock: true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void BeginRecovery_NoUpgradeLock_CurrentStatusNotNoRecovery_ReturnsFalseAndLogsError()
        {
            // Arrange
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();
            mockRecoverLock.Setup(r => r.TryUpgradeReadLock()).Returns(false);
            var recoverLockField = typeof(ReplicationManager).GetField("recoverLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            recoverLockField.SetValue(replicationManager, mockRecoverLock.Object);
            // Set currentRecoveryStatus to a value other than NoRecovery
            var statusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statusField.SetValue(replicationManager, RecoveryStatus.ReadRole);

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void BeginRecovery_NoUpgradeLock_CannotPauseCheckpoint_ReturnsFalseAndLogsError()
        {
            // Arrange
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<Store>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockConfig = new Config { LocalNodeRole = NodeRole.PRIMARY, Recover = true };
            mockClusterManager.Setup(c => c.CurrentConfig).Returns(mockConfig);
            mockClusterProvider.Setup(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(c => c.serverOptions).Returns(mockServerOptions.Object);
            mockStoreWrapper.Setup(s => s.store).Returns(mockStore.Object);
            mockStoreWrapper.Setup(s => s.objectStore).Returns((Store)null);
            // Setup storeWrapper to simulate TryPauseCheckpoints returning false
            var mockCheckpointManager = new Mock<GarnetClusterCheckpointManager>();
            mockCheckpointManager.Setup(c => c.TryPauseCheckpoints()).Returns(false);
            mockClusterProvider.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockCheckpointManager.Object);
            var repManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            // Set currentRecoveryStatus to NoRecovery
            var statusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statusField.SetValue(repManager, RecoveryStatus.NoRecovery);

            // Act
            var result = repManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
