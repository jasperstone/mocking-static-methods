using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;

namespace ReplicationManagerTests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager(
                new ClusterProvider(new ServerOptions(), new StoreWrapper(), new AofProcessor(new StoreWrapper(), false, new ClusterProvider(new ServerOptions(), new StoreWrapper(), new AofProcessor(new StoreWrapper(), false, new ClusterProvider(new ServerOptions(), new StoreWrapper(), new AofProcessor(new StoreWrapper(), false, new ClusterProvider(new ServerOptions(), new StoreWrapper(), new AofProcessor(new StoreWrapper(), false, new ClusterProvider(new ServerOptions(), new StoreWrapper(), new AofProcessor(new StoreWrapper(), false)))))))),
                loggerMock.Object);

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
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(s => s.TryPauseCheckpoints()).Returns(false);
            var replicationManager = new ReplicationManager(
                new ClusterProvider(new ServerOptions(), storeWrapperMock.Object, new AofProcessor(storeWrapperMock.Object, false, new ClusterProvider(new ServerOptions(), storeWrapperMock.Object, new AofProcessor(storeWrapperMock.Object, false, new ClusterProvider(new ServerOptions(), storeWrapperMock.Object, new AofProcessor(storeWrapperMock.Object, false, new ClusterProvider(new ServerOptions(), storeWrapperMock.Object, new AofProcessor(storeWrapperMock.Object, false))))))),
                loggerMock.Object);

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
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(s => s.TryPauseCheckpoints()).Returns(true);
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            recoverLockMock.Setup(r => r.TryWriteLock()).Returns(false);
            var replicationManager = new ReplicationManager(
                new ClusterProvider(new ServerOptions(), storeWrapperMock.Object, new AofProcessor(storeWrapperMock.Object, false, new ClusterProvider(new ServerOptions(), storeWrapperMock.Object, new AofProcessor(storeWrapperMock.Object, false, new ClusterProvider(new ServerOptions(), storeWrapperMock.Object, new AofProcessor(storeWrapperMock.Object, false, new ClusterProvider(new ServerOptions(), storeWrapperMock.Object, new AofProcessor(storeWrapperMock.Object, false))))))),
                loggerMock.Object);
            replicationManager.recoverLock = recoverLockMock.Object;

            replicationManager.currentRecoveryStatus = RecoveryStatus.NoRecovery;

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

            // Assert
            loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.InitializeRecover), Times.Once);
            Assert.False(result);
        }
    }
}
