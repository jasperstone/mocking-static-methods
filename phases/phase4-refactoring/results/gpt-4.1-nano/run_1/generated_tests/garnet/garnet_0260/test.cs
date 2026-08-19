using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_ShouldLogError_WhenRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<ClusterConfig>();
            var mockStore = new Mock<Store>();
            var mockObjectStore = new Mock<ObjectStore>();
            var mockCheckpointManager = new Mock<GarnetClusterCheckpointManager>();
            var mockDeviceFactory = new Mock<IDeviceFactory>();
            var mockFileDescriptor = new Mock<FileDescriptor>();

            // Setup minimal dependencies
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(mockCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.PRIMARY });
            mockStoreWrapper.Setup(sw => sw.store).Returns(mockStore.Object);
            mockStoreWrapper.Setup(sw => sw.objectStore).Returns(mockObjectStore.Object);
            mockCheckpointManager.Setup(cm => cm.RecoveredSafeAofAddress).Returns(0);
            mockCheckpointManager.Setup(cm => cm.CurrentSafeAofAddress).Returns(0);
            mockStore.Setup(s => s.CheckpointManager).Returns(mockCheckpointManager.Object);
            mockObjectStore.Setup(os => os.CheckpointManager).Returns(mockCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.GetInitializedDeviceFactory(It.IsAny<string>())).Returns(new Mock<IDeviceFactory>().Object);
            var mockDeviceFactory = new Mock<IDeviceFactory>();
            mockClusterProvider.Setup(cp => cp.GetInitializedDeviceFactory(It.IsAny<string>())).Returns(mockDeviceFactory.Object);
            mockDeviceFactory.Setup(df => df.Get(It.IsAny<FileDescriptor>())).Returns(mockFileDescriptor.Object);
            mockFileDescriptor.Setup(fd => fd.GetFileSize(0)).Returns(1);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { Recover = true });
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            // Set currentRecoveryStatus to a value that triggers LogError
            var repManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            repManager.currentRecoveryStatus = RecoveryStatus.ReadRole;

            // Act
            var result = repManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
            Assert.False(result);
        }
    }
}
