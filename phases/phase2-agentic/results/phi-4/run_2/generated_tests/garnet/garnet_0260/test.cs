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
        public void BeginRecovery_ShouldLogError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.ReadRole
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenCannotPauseCheckpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();

            storeWrapperMock.Setup(x => x.TryPauseCheckpoints()).Returns(false);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.NoRecovery
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenCannotAcquireRecoverLock()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();

            recoverLockMock.Setup(x => x.TryReadLock()).Returns(false);
            recoverLockMock.Setup(x => x.TryWriteLock()).Returns(false);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.NoRecovery
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
            Assert.False(result);
        }
    }
}
