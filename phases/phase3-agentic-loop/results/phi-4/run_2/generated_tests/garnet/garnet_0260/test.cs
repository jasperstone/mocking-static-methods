using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.cluster.Server.Replication; // Added namespace for ReplicationManager and related types

[assembly: InternalsVisibleTo("Garnet.cluster.Server.Replication.Tests")] // Ensure test project can access internal members

namespace Garnet.cluster.Server.Replication.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();

            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(s => s.TryPauseCheckpoints()).Returns(true);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.ReadRole,
                recoverLock = recoverLockMock.Object
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCannotAcquireCheckpointLock()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();

            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(s => s.TryPauseCheckpoints()).Returns(false);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.NoRecovery,
                recoverLock = recoverLockMock.Object
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCannotAcquireRecoverLock()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();

            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(s => s.TryPauseCheckpoints()).Returns(true);

            recoverLockMock.Setup(r => r.TryReadLock()).Returns(false);
            recoverLockMock.Setup(r => r.TryWriteLock()).Returns(false);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object)
            {
                currentRecoveryStatus = RecoveryStatus.NoRecovery,
                recoverLock = recoverLockMock.Object
            };

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }
    }
}
