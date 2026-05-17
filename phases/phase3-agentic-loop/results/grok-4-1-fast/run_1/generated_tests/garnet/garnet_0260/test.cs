using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_WhenCurrentRecoveryStatusNotNoRecovery_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(x => x.TryPauseCheckpoints()).Returns(true);

            var replicationManager = new TestReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            replicationManager.currentRecoveryStatus = RecoveryStatus.InitializeRecover;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.ReadRole),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_WhenTryPauseCheckpointsFails_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(x => x.TryPauseCheckpoints()).Returns(false);

            var replicationManager = new TestReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", RecoveryStatus.ReadRole),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_WhenRecoverLockCannotBeAcquired_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(x => x.TryPauseCheckpoints()).Returns(true);
            mockStoreWrapper.Setup(x => x.ResumeCheckpoints());

            var replicationManager = new TestReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            replicationManager.recoverLock = new FailingLock();

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.ReadRole),
                Times.Once);
            mockStoreWrapper.Verify(x => x.ResumeCheckpoints(), Times.Once);
        }
    }

    internal class TestReplicationManager : ReplicationManager
    {
        public TestReplicationManager(ClusterProvider clusterProvider, ILogger logger) : base(clusterProvider, logger)
        {
            this.recoverLock ??= new SingleWriterMultiReaderLock();
            this.currentRecoveryStatus = RecoveryStatus.NoRecovery;
        }
    }

    internal class FailingLock : SingleWriterMultiReaderLock
    {
        public override bool TryReadLock() => false;
        public override bool TryWriteLock() => false;
        public override bool TryUpgradeReadLock() => false;
    }
}
