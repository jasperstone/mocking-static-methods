using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using System.Threading.Tasks;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task AcquireCheckpointEntryAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var replicationManagerMock = new Mock<ReplicationManager>();

        clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);

        var replicaSyncSession = new ReplicaSyncSession(
            storeWrapperMock.Object,
            clusterProviderMock.Object,
            logger: loggerMock.Object);

        // Act
        await replicaSyncSession.AcquireCheckpointEntryAsync();

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }
}
