using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task SendCheckpointAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapper = new StoreWrapper(
            new Store(),
            new ServerOptions(),
            new ClusterManager(),
            new ReplicationManager(),
            loggerMock.Object);
        var clusterProvider = new ClusterProvider(
            new ClusterManager(),
            new ReplicationManager(),
            new ServerOptions(),
            loggerMock.Object);
        var replicaSyncSession = new ReplicaSyncSession(
            storeWrapper,
            clusterProvider,
            logger: loggerMock.Object);

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<FormattedLogValues>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<FormattedLogValues, Exception, string>>()),
            Times.AtLeastOnce);
    }
}
