using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading;

public class ReplicationManagerTests
{
    [Fact]
    public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager(new ClusterProvider(), loggerMock.Object);
        replicationManager.GetType().GetProperty("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(replicationManager, RecoveryStatus.InitializeRecover);

        // Act
        var result = replicationManager.GetType().GetMethod("BeginRecovery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.ReadRole, false });

        // Assert
        loggerMock.Verify(l => l.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        Assert.False((bool)result);
    }

    [Fact]
    public void BeginRecovery_LogsError_WhenCannotAcquireCheckpointLock()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager(new ClusterProvider(), loggerMock.Object);
        replicationManager.GetType().GetProperty("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(replicationManager, RecoveryStatus.NoRecovery);

        // Act
        var result = replicationManager.GetType().GetMethod("BeginRecovery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.ReadRole, false });

        // Assert
        loggerMock.Verify(l => l.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        Assert.False((bool)result);
    }

    [Fact]
    public void BeginRecovery_LogsError_WhenCannotAcquireRecoverLock()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager(new ClusterProvider(), loggerMock.Object);
        replicationManager.GetType().GetProperty("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(replicationManager, RecoveryStatus.NoRecovery);

        // Act
        var result = replicationManager.GetType().GetMethod("BeginRecovery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(replicationManager, new object[] { RecoveryStatus.ReadRole, false });

        // Assert
        loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        Assert.False((bool)result);
    }
}
