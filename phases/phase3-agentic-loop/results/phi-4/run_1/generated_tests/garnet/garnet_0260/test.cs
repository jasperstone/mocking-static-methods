using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster.Server.Replication;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        private readonly Mock<ILogger> loggerMock = new();
        private readonly Mock<ClusterProvider> clusterProviderMock = new();
        private readonly Mock<StoreWrapper> storeWrapperMock = new();
        private readonly Mock<SingleWriterMultiReaderLock> recoverLockMock = new();

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.ReadRole,
                recoverLock = recoverLockMock.Object,
                clusterProvider = new ClusterProvider
                {
                    storeWrapper = storeWrapperMock.Object
                }
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenCannotPauseCheckpoints()
        {
            // Arrange
            storeWrapperMock.Setup(s => s.TryPauseCheckpoints()).Returns(false);
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.NoRecovery,
                recoverLock = recoverLockMock.Object,
                clusterProvider = new ClusterProvider
                {
                    storeWrapper = storeWrapperMock.Object
                }
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_ShouldLogError_WhenCannotAcquireRecoverLock()
        {
            // Arrange
            recoverLockMock.Setup(r => r.TryReadLock()).Returns(false);
            recoverLockMock.Setup(r => r.TryWriteLock()).Returns(false);
            storeWrapperMock.Setup(s => s.TryPauseCheckpoints()).Returns(true);
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.NoRecovery,
                recoverLock = recoverLockMock.Object,
                clusterProvider = new ClusterProvider
                {
                    storeWrapper = storeWrapperMock.Object
                }
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
