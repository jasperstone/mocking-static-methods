using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using System.Reflection;

namespace ReplicationManagerTests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);
            var fieldInfo = typeof(ReplicationManager).GetField("currentRecoveryStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(replicationManager, RecoveryStatus.InitializeRecover);

            // Act
            var result = (bool)typeof(ReplicationManager).GetMethod("BeginRecovery", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.NoRecovery, false });

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
            storeWrapperMock.Setup(sw => sw.TryPauseCheckpoints()).Returns(false);
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);
            var fieldInfo = typeof(ReplicationManager).GetField("currentRecoveryStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(replicationManager, RecoveryStatus.NoRecovery);

            // Act
            var result = (bool)typeof(ReplicationManager).GetMethod("BeginRecovery", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.InitializeRecover, false });

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
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);
            var fieldInfo = typeof(ReplicationManager).GetField("currentRecoveryStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(replicationManager, RecoveryStatus.NoRecovery);
            var recoverLockFieldInfo = typeof(ReplicationManager).GetField("recoverLock", BindingFlags.NonPublic | BindingFlags.Instance);
            recoverLockFieldInfo.SetValue(replicationManager, new SingleWriterMultiReaderLock());
            recoverLockFieldInfo.GetValue(replicationManager).TryWriteLock(); // Lock is already acquired

            // Act
            var result = (bool)typeof(ReplicationManager).GetMethod("BeginRecovery", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.InitializeRecover, false });

            // Assert
            loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.InitializeRecover), Times.Once);
            Assert.False(result);
        }
    }
}
