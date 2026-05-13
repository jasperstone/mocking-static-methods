using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.cluster.Server.Replication;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_ShouldLogError_WhenUpgradeLockFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            recoverLockMock.Setup(l => l.TryUpgradeReadLock()).Returns(false);

            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                recoverLock = recoverLockMock.Object
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: true);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                recoverLock = recoverLockMock.Object,
                currentRecoveryStatus = RecoveryStatus.ReadRole
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenCheckpointLockFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(s => s.TryPauseCheckpoints()).Returns(false);

            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                recoverLock = recoverLockMock.Object,
                currentRecoveryStatus = RecoveryStatus.NoRecovery
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenRecoverLockFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            recoverLockMock.Setup(l => l.TryReadLock()).Returns(false);

            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                recoverLock = recoverLockMock.Object,
                currentRecoveryStatus = RecoveryStatus.NoRecovery
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
