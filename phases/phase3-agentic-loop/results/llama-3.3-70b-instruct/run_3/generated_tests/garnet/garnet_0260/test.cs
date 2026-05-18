using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager(new ClusterProvider(new StoreWrapper()), loggerMock.Object);

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

            // Assert
            loggerMock.Verify(l => l.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.InitializeRecover), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCannotAcquireCheckpointLock()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(sw => sw.TryPauseCheckpoints()).Returns(false);
            var replicationManager = new ReplicationManager(new ClusterProvider(storeWrapperMock.Object), loggerMock.Object);

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

            // Assert
            loggerMock.Verify(l => l.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", RecoveryStatus.InitializeRecover), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCannotAcquireRecoverLock()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(sw => sw.TryPauseCheckpoints()).Returns(true);
            var replicationManager = new ReplicationManager(new ClusterProvider(storeWrapperMock.Object), loggerMock.Object);
            replicationManager.recoverLock = new SingleWriterMultiReaderLock();
            replicationManager.recoverLock.TryWriteLock();

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

            // Assert
            loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.InitializeRecover), Times.Once);
            Assert.False(result);
        }
    }
}
