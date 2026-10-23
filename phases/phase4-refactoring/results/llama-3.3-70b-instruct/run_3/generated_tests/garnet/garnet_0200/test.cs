using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task TestLogInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var replicaSyncSession = new ReplicaSyncSession(
            storeWrapperMock.Object,
            clusterProviderMock.Object,
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
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }
}
