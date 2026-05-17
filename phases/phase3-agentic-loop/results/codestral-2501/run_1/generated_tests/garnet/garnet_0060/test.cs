using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.client;
using System.Threading.Tasks;
using System.Threading;
using System;

public class ReplicaFailoverSessionTests
{
    [Fact]
    public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReplicaOfRespIsNotOK()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FailoverSession>>();
        var mockClient = new Mock<GarnetClient>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockClusterManager = new Mock<ClusterManager>();
        var mockReplicationManager = new Mock<ReplicationManager>();
        var mockStoreWrapper = new Mock<StoreWrapper>();

        var failoverSession = new FailoverSession(
            mockClusterProvider.Object,
            mockClusterManager.Object,
            mockReplicationManager.Object,
            mockStoreWrapper.Object,
            mockLogger.Object
        );

        var replicaId = "replica1";
        var configByteArray = new byte[] { 1, 2, 3 };
        var replicaOfResp = "NOT_OK";

        mockClient.Setup(client => client.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                  .ReturnsAsync(replicaOfResp);

        // Act
        await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

        // Assert
        mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);
    }
}
