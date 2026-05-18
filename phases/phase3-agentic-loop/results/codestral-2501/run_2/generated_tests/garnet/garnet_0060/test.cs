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
            mockLogger.Object,
            CancellationToken.None);

        var replicaId = "replica1";
        var configByteArray = new byte[] { 1, 2, 3 };
        var localAddress = "127.0.0.1";
        var localPort = 12345;
        var failoverTimeout = TimeSpan.FromSeconds(10);

        mockClient.Setup(client => client.ReplicaOf(localAddress, localPort))
                  .ReturnsAsync("NOT_OK");

        // Act
        await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

        // Assert
        mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas Error: replica1 NOT_OK")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
