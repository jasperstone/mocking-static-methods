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
    public async Task BroadcastConfigAndRequestAttachAsync_LogsCritical_WhenExceptionThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FailoverSession>>();
        var mockClient = new Mock<GarnetClient>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockClusterManager = new Mock<ClusterManager>();
        var mockReplicationManager = new Mock<ReplicationManager>();
        var mockStoreWrapper = new Mock<StoreWrapper>();

        var replicaId = "replica1";
        var configByteArray = new byte[] { 1, 2, 3 };

        mockClusterProvider.Setup(cp => cp.ClusterManager).Returns(mockClusterManager.Object);
        mockClusterProvider.Setup(cp => cp.ReplicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.StoreWrapper).Returns(mockStoreWrapper.Object);

        var failoverSession = new FailoverSession(
            mockClusterProvider.Object,
            mockLogger.Object,
            TimeSpan.FromSeconds(10),
            new CancellationTokenSource()
        );

        mockClient.Setup(client => client.GossipAsync(configByteArray))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

        // Assert
        mockLogger.Verify(
            logger => logger.LogCritical(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
