using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

public class ReplicationManagerTests
{
    [Fact]
    public void BeginRecovery_NoRecovery_ReturnsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager(new ClusterProvider(), loggerMock.Object);

        // Act
        var result = replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BeginRecovery_RecoveryInProgress_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager(new ClusterProvider(), loggerMock.Object);
        replicationManager.currentRecoveryStatus = RecoveryStatus.InitializeRecover;

        // Act
        replicationManager.BeginRecovery(RecoveryStatus.ClusterReplicate, false);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void BeginRecovery_CannotAcquireCheckpointLock_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager(new ClusterProvider(), loggerMock.Object);
        replicationManager.storeWrapper = new StoreWrapper { TryPauseCheckpoints = () => false };

        // Act
        replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void BeginRecovery_CannotAcquireRecoverLock_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager(new ClusterProvider(), loggerMock.Object);
        replicationManager.recoverLock = new SingleWriterMultiReaderLock { TryReadLock = () => false };

        // Act
        replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }
}
