using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

public class ReplicationManagerTests
{
    [Fact]
    public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
        var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);
        replicationManager.GetType().GetProperty("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(replicationManager, RecoveryStatus.InitializeRecover);

        // Act
        var result = replicationManager.GetType().GetMethod("BeginRecovery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.NoRecovery, false });

        // Assert
        loggerMock.Verify(l => l.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.NoRecovery), Times.Once);
        Assert.False((bool)result);
    }

    [Fact]
    public void BeginRecovery_LogsError_WhenCannotAcquireCheckpointLock()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.TryPauseCheckpoints()).Returns(false);
        clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
        var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);
        replicationManager.GetType().GetProperty("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(replicationManager, RecoveryStatus.NoRecovery);

        // Act
        var result = replicationManager.GetType().GetMethod("BeginRecovery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.InitializeRecover, false });

        // Assert
        loggerMock.Verify(l => l.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", RecoveryStatus.InitializeRecover), Times.Once);
        Assert.False((bool)result);
    }

    [Fact]
    public void BeginRecovery_LogsError_WhenCannotAcquireRecoverLock()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.TryPauseCheckpoints()).Returns(true);
        clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
        var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);
        replicationManager.GetType().GetProperty("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(replicationManager, RecoveryStatus.NoRecovery);
        var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
        recoverLockMock.Setup(rl => rl.TryWriteLock()).Returns(false);
        replicationManager.GetType().GetProperty("recoverLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(replicationManager, recoverLockMock.Object);

        // Act
        var result = replicationManager.GetType().GetMethod("BeginRecovery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.InitializeRecover, false });

        // Assert
        loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.InitializeRecover), Times.Once);
        Assert.False((bool)result);
    }
}
