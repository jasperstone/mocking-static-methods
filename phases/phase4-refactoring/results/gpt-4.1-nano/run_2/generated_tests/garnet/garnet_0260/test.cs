using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;
using System.Reflection;

namespace Garnet.test
{
    public class ReplicationManagerLoggingTests
    {
        [Fact]
        public void BeginRecovery_ShouldLogError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<ClusterConfig>();
            var mockStore = new Mock<Store>();
            var mockObjectStore = new Mock<ObjectStore>();
            var mockCheckpointManager = new Mock<GarnetClusterCheckpointManager>();
            var mockDeviceFactory = new Mock<IDeviceFactory>();
            var mockDevice = new Mock<IFileDescriptor>();
            var mockDevicePool = new Mock<IDevicePool>();
            var mockReplicationLogCheckpointManager = new Mock<ReplicationLogCheckpointManager>();

            // Setup minimal required properties and methods
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockReplicationLogCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main))
                .Returns(mockReplicationLogCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object))
                .Returns(mockReplicationLogCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.PRIMARY });
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns((IAppendOnlyFile)null);
            mockStoreWrapper.Setup(sw => sw.objectStore).Returns((ObjectStore)null);

            // Instantiate ReplicationManager
            var repManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            // Set internal fields
            typeof(ReplicationManager).GetField("recoverLock", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(repManager, new SingleWriterMultiReaderLock());
            typeof(ReplicationManager).GetField("recoveryStateChangeLock", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(repManager, new SingleWriterMultiReaderLock());
            // Set currentRecoveryStatus to a value other than NoRecovery
            typeof(ReplicationManager).GetProperty("currentRecoveryStatus", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(repManager, RecoveryStatus.ReadRole);

            // Act
            var result = repManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
