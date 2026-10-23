using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_ShouldLogWarning_WhenReplicaOfRespIsNotOK()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGarnetClient = new Mock<GarnetClient>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockOldConfig = new Mock<ClusterConfig>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);

            var failoverSession = new FailoverSession(
                mockClusterProvider.Object,
                mockOldConfig.Object,
                mockLogger.Object,
                new CancellationTokenSource().Token);

            var replicaId = "replica1";
            var configByteArray = new byte[] { };

            mockGarnetClient.Setup(client => client.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("NOT_OK");

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
