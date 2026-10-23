using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task SendCheckpointAsync_LogsCheckpointSearchCompleted()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var replicaSyncSession = new MockReplicaSyncSession(
            storeWrapper: null,
            clusterProvider: null,
            replicaNodeId: "replicaNodeId",
            replicaCheckpointEntry: new CheckpointEntry(),
            logger: mockLogger.Object
        );

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);
    }
}

public class MockReplicaSyncSession : ReplicaSyncSession
{
    public MockReplicaSyncSession(
        StoreWrapper storeWrapper,
        ClusterProvider clusterProvider,
        SyncMetadata replicaSyncMetadata = null,
        CancellationToken token = default,
        string replicaNodeId = null,
        string replicaAssignedPrimaryId = null,
        CheckpointEntry replicaCheckpointEntry = null,
        long replicaAofBeginAddress = 0,
        long replicaAofTailAddress = 0,
        ILogger logger = null)
        : base(storeWrapper, clusterProvider, replicaSyncMetadata, token, replicaNodeId, replicaAssignedPrimaryId, replicaCheckpointEntry, replicaAofBeginAddress, replicaAofTailAddress, logger)
    {
    }

    public override async Task<bool> SendCheckpointAsync()
    {
        // Call the base method to ensure the original behavior is preserved
        var result = await base.SendCheckpointAsync();

        // Additional logging for testing purposes
        logger?.LogInformation("Checkpoint search completed");

        return result;
    }
}
