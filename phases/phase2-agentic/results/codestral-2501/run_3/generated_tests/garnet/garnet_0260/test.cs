using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_WhenCurrentRecoveryStatusIsNotNoRecovery_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();

            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object)
            {
                currentRecoveryStatus = RecoveryStatus.InitializeRecover,
                recoverLock = mockRecoverLock.Object,
                clusterProvider = mockClusterProvider.Object
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_WhenCannotAcquireCheckpointLock_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();

            mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(x => x.TryPauseCheckpoints()).Returns(false);

            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object)
            {
                currentRecoveryStatus = RecoveryStatus.NoRecovery,
                recoverLock = mockRecoverLock.Object,
                clusterProvider = mockClusterProvider.Object
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire checkpoint lock")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_WhenCannotAcquireRecoverLock_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();

            mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(x => x.TryPauseCheckpoints()).Returns(true);
            mockRecoverLock.Setup(x => x.TryReadLock()).Returns(false);

            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object)
            {
                currentRecoveryStatus = RecoveryStatus.NoRecovery,
                recoverLock = mockRecoverLock.Object,
                clusterProvider = mockClusterProvider.Object
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire recover lock")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }
    }
}
