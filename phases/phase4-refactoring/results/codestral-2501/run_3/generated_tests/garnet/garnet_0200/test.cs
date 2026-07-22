using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Threading;
using System;
using Garnet.common;
using Garnet.server;
using Garnet.client;
using System.Net;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task SendCheckpointAsync_LogsInformation_WhenSendingCheckpointMetadata()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockReplicationManager = new Mock<ReplicationManager>();
        var mockCkptManager = new Mock<ReplicationLogCheckpointManager>();
        var mockClusterManager = new Mock<ClusterManager>();
        var mockGarnetClientSession = new Mock<GarnetClientSession>();

        mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(mockCkptManager.Object);
        mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicaSyncTimeout = TimeSpan.FromSeconds(10) });

        var replicaSyncSession = new Mock<ReplicaSyncSession>(
            mockStoreWrapper.Object,
            mockClusterProvider.Object,
            logger: mockLogger.Object);

        var fileToken = Guid.NewGuid();
        var fileType = CheckpointFileType.STORE_SNAPSHOT;

        mockGarnetClientSession.Setup(gcs => gcs.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
            .ReturnsAsync("OK");

        // Act
        await replicaSyncSession.Object.SendCheckpointAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}
