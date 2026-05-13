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
            // Setup minimal required properties and methods
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<Store>();
            var mockObjectStore = new Mock<Store>();
            var mockCheckpointManager = new Mock<GarnetClusterCheckpointManager>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<Config>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockGetDeviceFactory = new Mock<Func<FileDescriptor, IDevice>>();

            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions.CheckpointDir).Returns("/tmp");
            mockClusterProvider.Setup(cp => cp.GetInitializedDeviceFactory(It.IsAny<string>()))
                .Returns(() => new Mock<IDevice>().Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new Config { LocalNodeRole = NodeRole.PRIMARY });
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());

            // Instantiate ReplicationManager with mocks
            replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenRecoveryStatusIsNotNoRecoveryOrReadRole()
        {
            // Arrange
            var mockLock = new Mock<SingleWriterMultiReaderLock>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<Config>();
            mockConfig.Setup(c => c.LocalNodeRole).Returns(NodeRole.PRIMARY);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockConfig.Object);
            // Force currentRecoveryStatus to a value that triggers LogError
            replicationManager.currentRecoveryStatus = RecoveryStatus.Recovering;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.Recovering, false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<string>(), It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
