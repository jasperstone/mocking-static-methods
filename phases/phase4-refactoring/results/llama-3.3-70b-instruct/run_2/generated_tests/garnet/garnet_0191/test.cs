using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task TestLogError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<Garnet.server.StoreWrapper>();
        var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
        var replicaSyncMetadataMock = new Mock<Garnet.cluster.SyncMetadata>();
        var token = new CancellationToken();
        var replicaNodeId = "replicaNodeId";
        var replicaAssignedPrimaryId = "replicaAssignedPrimaryId";
        var replicaCheckpointEntry = new Garnet.cluster.CheckpointEntry();
        var replicaAofBeginAddress = 0L;
        var replicaAofTailAddress = 0L;

        storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(new Garnet.server.AppendOnlyFile { BeginAddress = 10L });

        var replicaSyncSession = new Garnet.cluster.ReplicaSyncSession(
            storeWrapperMock.Object,
            clusterProviderMock.Object,
            replicaSyncMetadataMock.Object,
            token,
            replicaNodeId,
            replicaAssignedPrimaryId,
            replicaCheckpointEntry,
            replicaAofBeginAddress,
            replicaAofTailAddress,
            loggerMock.Object);

        // Act
        var syncFromAofAddress = 5L;
        var possibleAofDataLoss = false;
        var localEntry = new Garnet.cluster.CheckpointEntry();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), syncFromAofAddress, storeWrapperMock.Object.appendOnlyFile.BeginAddress), Times.Never);

        // Act
        replicaSyncSession.logger?.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", syncFromAofAddress, storeWrapperMock.Object.appendOnlyFile.BeginAddress);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), syncFromAofAddress, storeWrapperMock.Object.appendOnlyFile.BeginAddress), Times.Once);
    }
}
