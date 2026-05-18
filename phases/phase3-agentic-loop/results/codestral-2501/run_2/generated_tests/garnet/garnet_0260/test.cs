using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;

public class ReplicationManagerTests
{
    [Fact]
    public void BeginRecovery_WhenUpgradeLockFails_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();

        mockRecoverLock.Setup(lockObj => lockObj.TryUpgradeReadLock()).Returns(false);

        var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object)
        {
            recoverLock = mockRecoverLock.Object,
            currentRecoveryStatus = RecoveryStatus.NoRecovery
        };

        // Act
        var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, true);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.False(result);
    }

    [Fact]
    public void BeginRecovery_WhenPauseCheckpointsFails_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();

        mockClusterProvider.Setup(provider => provider.storeWrapper).Returns(mockStoreWrapper.Object);
        mockStoreWrapper.Setup(wrapper => wrapper.TryPauseCheckpoints()).Returns(false);

        var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object)
        {
            recoverLock = mockRecoverLock.Object,
            currentRecoveryStatus = RecoveryStatus.NoRecovery
        };

        // Act
        var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire checkpoint lock")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.False(result);
    }

    [Fact]
    public void BeginRecovery_WhenAcquireRecoverLockFails_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockRecoverLock = new Mock<SingleWriterMultiReaderLock>();

        mockClusterProvider.Setup(provider => provider.storeWrapper).Returns(mockStoreWrapper.Object);
        mockStoreWrapper.Setup(wrapper => wrapper.TryPauseCheckpoints()).Returns(true);
        mockRecoverLock.Setup(lockObj => lockObj.TryReadLock()).Returns(false);

        var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object)
        {
            recoverLock = mockRecoverLock.Object,
            currentRecoveryStatus = RecoveryStatus.NoRecovery
        };

        // Act
        var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire recover lock")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.False(result);
    }
}
