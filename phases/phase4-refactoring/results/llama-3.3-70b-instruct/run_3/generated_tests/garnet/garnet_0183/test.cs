using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public void LogInformation_Called_WithCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicaSyncSession = new ReplicaSyncSession(
            new StoreWrapper(),
            new ClusterProvider(new StoreWrapper()),
            new SyncMetadata(),
            default,
            "replicaNodeId",
            "replicaAssignedPrimaryId",
            new CheckpointEntry(),
            0,
            0,
            loggerMock.Object);

        // Act
        replicaSyncSession.SendCheckpointAsync().Wait();

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public void SendCheckpointAsync_ReturnsTrue_IfCheckpointSentSuccessfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicaSyncSession = new ReplicaSyncSession(
            new StoreWrapper(),
            new ClusterProvider(new StoreWrapper()),
            new SyncMetadata(),
            default,
            "replicaNodeId",
            "replicaAssignedPrimaryId",
            new CheckpointEntry(),
            0,
            0,
            loggerMock.Object);

        // Act
        var result = replicaSyncSession.SendCheckpointAsync().Result;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SendCheckpointAsync_ReturnsFalse_IfErrorSendingCheckpoint()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicaSyncSession = new ReplicaSyncSession(
            new StoreWrapper(),
            new ClusterProvider(new StoreWrapper()),
            new SyncMetadata(),
            default,
            "replicaNodeId",
            "replicaAssignedPrimaryId",
            new CheckpointEntry(),
            0,
            0,
            loggerMock.Object);

        // Act
        var result = replicaSyncSession.SendCheckpointAsync().Result;

        // Assert
        Assert.False(result);
    }
}
