using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.cluster;
using System.Reflection;

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

        var session = new FailoverSession(
            mockClusterProvider.Object,
            new FailoverOption(),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(60),
            new LightEpoch(),
            true,
            "",
            -1,
            mockLogger.Object
        );

        var replicaId = "replica1";
        var configByteArray = new byte[] { };

        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);

        mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var method = typeof(FailoverSession).GetMethod("BroadcastConfigAndRequestAttachAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method.Invoke(session, new object[] { replicaId, configByteArray });

        // Assert
        mockLogger.Verify(
            x => x.LogCritical(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
