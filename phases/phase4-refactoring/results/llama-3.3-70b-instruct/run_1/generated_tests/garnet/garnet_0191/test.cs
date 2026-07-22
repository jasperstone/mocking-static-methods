using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public void LogError_Called_When_SyncFromAofAddress_Is_Less_Than_BeginAofAddress()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var replicaSyncSession = new ReplicaSyncSession(
            storeWrapperMock.Object,
            clusterProviderMock.Object,
            replicaNodeId: "replicaNodeId",
            replicaAssignedPrimaryId: "replicaAssignedPrimaryId",
            replicaCheckpointEntry: new CheckpointEntry(),
            replicaAofBeginAddress: 0,
            replicaAofTailAddress: 0,
            logger: loggerMock.Object);

        // Act
        replicaSyncSession.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 0, 1);

        // Assert
        loggerMock.Verify(l => l.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 0, 1), Times.Once);
    }
}
