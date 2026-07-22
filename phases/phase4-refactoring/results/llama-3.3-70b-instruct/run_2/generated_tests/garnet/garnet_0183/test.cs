using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task SendCheckpointAsync_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicaSyncSession = new ReplicaSyncSession(
            new StoreWrapper(),
            new ClusterProvider(),
            null,
            default,
            "replicaNodeId",
            "replicaAssignedPrimaryId",
            new CheckpointEntry(),
            0,
            0,
            loggerMock.Object);

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
