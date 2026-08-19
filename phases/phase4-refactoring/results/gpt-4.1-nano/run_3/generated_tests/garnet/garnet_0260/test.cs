using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
using System;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_ShouldLogError_WhenRecoveryStatusIsNotNoRecoveryOrReadRole()
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
            var mockDevice = new Mock<IDevice>();
            var mockFileDescriptor = new FileDescriptor("", "replication.conf");
            var mockDevicePool = new object();

            // Setup ClusterProvider
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new ClusterManager { CurrentConfig = new ClusterConfig { LocalNodeRole = NodeRole.PRIMARY } });
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(new StoreWrapper());
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(mockCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new ClusterManager { CurrentConfig = new ClusterConfig { LocalNodeRole = NodeRole.PRIMARY } });
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new ClusterManager { CurrentConfig = new ClusterConfig { LocalNodeRole = NodeRole.PRIMARY } });
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(new StoreWrapper());

            // Instantiate ReplicationManager
            var repManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);

            // Set currentRecoveryStatus to a value that triggers LogError
            repManager.currentRecoveryStatus = RecoveryStatus.RecoverFailed;

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
