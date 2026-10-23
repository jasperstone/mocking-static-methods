using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.cluster;

public class ReplicaFailoverSessionTests
{
    [Fact]
    public async Task BroadcastConfigAndRequestAttachAsync_ExceptionThrown_LogsCritical()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FailoverSession>>();
        var mockClient = new Mock<GarnetClient>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockClusterManager = new Mock<ClusterManager>();
        var mockReplicationManager = new Mock<ReplicationManager>();
        var mockStoreWrapper = new Mock<StoreWrapper>();

        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);

        var failoverSession = new FailoverSession(mockClusterProvider.Object, mockLogger.Object);

        var replicaId = "replica1";
        var configByteArray = new byte[] { 1, 2, 3 };

        mockClient.Setup(client => client.GossipAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

        // Assert
        mockLogger.Verify(
            logger => logger.LogCritical(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
