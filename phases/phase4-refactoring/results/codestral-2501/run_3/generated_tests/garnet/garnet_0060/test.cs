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
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_ShouldLogWarning_WhenReplicaOfRespIsNotOK()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClient = new Mock<GarnetClient>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockOldConfig = new Mock<ClusterConfig>();
            var mockNewConfig = new Mock<ClusterConfig>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();

            var replicaId = "replica1";
            var configByteArray = new byte[] { 1, 2, 3 };
            var replicaOfResp = "Error";

            mockOldConfig.Setup(c => c.LocalNodePrimaryId).Returns("primary1");
            mockOldConfig.Setup(c => c.LocalNodeIp).Returns("127.0.0.1");
            mockOldConfig.Setup(c => c.LocalNodePort).Returns(1234);

            mockNewConfig.Setup(c => c.GetReplicaIds("primary1")).Returns(new[] { "replica1" });
            mockNewConfig.Setup(c => c.ToByteArray()).Returns(configByteArray);

            mockClusterManager.Setup(c => c.CurrentConfig).Returns(mockNewConfig.Object);

            mockClusterProvider.Setup(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(c => c.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(c => c.storeWrapper).Returns(mockStoreWrapper.Object);

            mockClient.Setup(c => c.ReplicaOf("127.0.0.1", 1234)).Returns(Task.FromResult(replicaOfResp));

            var failoverSession = new FailoverSession(
                mockClusterProvider.Object,
                mockOldConfig.Object,
                mockLogger.Object,
                new CancellationTokenSource().Token,
                TimeSpan.FromSeconds(30)
            );

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}
