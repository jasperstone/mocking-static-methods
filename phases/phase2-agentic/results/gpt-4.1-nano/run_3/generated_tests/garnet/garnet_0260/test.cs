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
            var mockObjectStore = new Mock<ObjectStore>();
            var mockCheckpointManager = new Mock<GarnetClusterCheckpointManager>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<Config>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockGetDeviceFactory = new Mock<Func<FileDescriptor, IDevice>>();
            var mockDevice = new Mock<IDevice>();
            var mockGetInitializedDeviceFactory = new Mock<Func<string, IDevice>>();

            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions.CheckpointDir).Returns("/tmp");
            mockClusterProvider.Setup(cp => cp.GetInitializedDeviceFactory(It.IsAny<string>()))
                .Returns(() => mockDevice.Object);

            // Setup clusterManager.CurrentConfig.LocalNodeRole
            var mockConfigObj = new Mock<Config>();
            mockConfigObj.Setup(c => c.LocalNodeRole).Returns(NodeRole.PRIMARY);
            mockClusterManager.Setup(c => c.CurrentConfig).Returns(mockConfigObj.Object);

            // Instantiate ReplicationManager
            replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
        }

        [Fact]
        public void BeginRecovery_ShouldReturnFalse_WhenUpgradeLockFails()
        {
            // Arrange
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();
            mockRecoverLock.Setup(r => r.TryUpgradeReadLock()).Returns(false);
            // Inject mock lock
            var repManager = new PrivateObject(replicationManager);
            repManager.SetFieldOrProperty("recoverLock", mockRecoverLock.Object);

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_ShouldReturnFalse_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();
            mockRecoverLock.Setup(r => r.TryUpgradeReadLock()).Returns(true);
            var repManager = new PrivateObject(replicationManager);
            repManager.SetFieldOrProperty("recoverLock", mockRecoverLock.Object);
            // Set currentRecoveryStatus to a value other than NoRecovery
            repManager.SetFieldOrProperty("currentRecoveryStatus", RecoveryStatus.CheckpointRecoveredAtPrimary);

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenNotInNoRecoveryAndCheckpointLockFails()
        {
            // Arrange
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();
            mockRecoverLock.Setup(r => r.TryUpgradeReadLock()).Returns(true);
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<Config>();
            var mockServerOptions = new Mock<ServerOptions>();
            mockServerOptions.Setup(s => s.Recover).Returns(true);
            mockConfig.Setup(c => c.LocalNodeRole).Returns(NodeRole.PRIMARY);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            var repManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            // Set currentRecoveryStatus to a value other than NoRecovery
            var repObj = new PrivateObject(repManager);
            repObj.SetFieldOrProperty("currentRecoveryStatus", RecoveryStatus.CheckpointRecoveredAtPrimary);
            // Mock clusterProvider's storeWrapper to return false for TryPauseCheckpoints
            var mockStore = new Mock<Store>();
            mockStore.Setup(s => s.CheckpointManager).Returns(new Mock<GarnetClusterCheckpointManager>().Object);
            mockStoreWrapper.Setup(s => s.store).Returns(mockStore.Object);
            // Force logger to capture logs
            var loggerMock = new Mock<ILogger>();
            repObj.SetFieldOrProperty("logger", loggerMock.Object);

            // Act
            var result = repManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
