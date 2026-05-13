using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace ReplicationManagerTests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager(new ClusterProvider(), loggerMock.Object);
            replicationManager.currentRecoveryStatus = RecoveryStatus.InitializeRecover;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.NoRecovery, false);

            // Assert
            loggerMock.Verify(l => l.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.NoRecovery), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCannotAcquireCheckpointLock()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(cp => cp.storeWrapper.TryPauseCheckpoints()).Returns(false);
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);
            replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;

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
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(cp => cp.storeWrapper.TryPauseCheckpoints()).Returns(true);
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);
            replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            replicationManager.recoverLock = new SingleWriterMultiReaderLock();
            replicationManager.recoverLock.TryWriteLock = () => false;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

            // Assert
            loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.InitializeRecover), Times.Once);
            Assert.False(result);
        }
    }
}
