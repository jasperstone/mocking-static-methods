using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task SendCheckpointAsync_LogsInformation_WhenSendingCheckpointMetadata()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var replicaSyncMetadataMock = new Mock<SyncMetadata>();
        var replicaCheckpointEntryMock = new Mock<CheckpointEntry>();
        var gcsMock = new Mock<GarnetClientSession>(new IPEndPoint(IPAddress.Loopback, 0), null, null, null, null, null, loggerMock.Object);

        var replicaSyncSession = new ReplicaSyncSession(
            storeWrapperMock.Object,
            clusterProviderMock.Object,
            replicaSyncMetadataMock.Object,
            CancellationToken.None,
            "replicaNodeId",
            "replicaAssignedPrimaryId",
            replicaCheckpointEntryMock.Object,
            0,
            0,
            loggerMock.Object);

        var fileToken = Guid.NewGuid();
        var fileType = CheckpointFileType.STORE_SNAPSHOT;

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        loggerMock.Verify(
            logger => logger.LogInformation(
                "<Begin sending checkpoint metadata {fileToken} {fileType}",
                fileToken,
                fileType),
            Times.Once);

        loggerMock.Verify(
            logger => logger.LogInformation(
                "<Complete sending checkpoint metadata {fileToken} {fileType}",
                fileToken,
                fileType),
            Times.Once);
    }
}
