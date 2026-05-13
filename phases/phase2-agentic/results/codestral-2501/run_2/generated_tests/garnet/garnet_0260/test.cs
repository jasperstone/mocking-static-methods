using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;

public class ReplicationManagerTests
{
    [Fact]
    public void BeginRecovery_LogsErrorWhenRecoveryStatusIsNotNoRecovery()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicationManager>>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

        replicationManager.currentRecoveryStatus = RecoveryStatus.ClusterReplicate;

        // Act
        var result = replicationManager.BeginRecovery(RecoveryStatus.InitializeRecover, false);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.False(result);
    }
}
