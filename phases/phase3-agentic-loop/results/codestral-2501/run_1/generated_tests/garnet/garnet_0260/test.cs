using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;

public class ReplicationManagerTests
{
    [Fact]
    public void BeginRecovery_WhenRecoveryStatusIsNotNoRecovery_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ReplicationManager>>();
        var clusterProvider = new Mock<ClusterProvider>();
        var storeWrapper = new Mock<StoreWrapper>();
        var replicationManager = new ReplicationManager(clusterProvider.Object, mockLogger.Object);

        replicationManager.currentRecoveryStatus = RecoveryStatus.InitializeRecover;

        // Act
        var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.False(result);
    }
}
