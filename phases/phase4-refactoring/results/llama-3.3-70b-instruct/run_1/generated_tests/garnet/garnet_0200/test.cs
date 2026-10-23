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
        var storeWrapper = new StoreWrapper();
        var clusterProvider = new ClusterProvider(storeWrapper);
        var replicaSyncSession = new ReplicaSyncSession(
            storeWrapper,
            clusterProvider,
            replicaNodeId: "replicaNodeId",
            replicaAssignedPrimaryId: "replicaAssignedPrimaryId",
            replicaCheckpointEntry: new CheckpointEntry(),
            logger: loggerMock.Object);

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }
}
