using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.server;

namespace Garnet.cluster.Tests
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public void LogWarning_WhenGossipTaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockGarnetClient = new Mock<GarnetClient>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockGossipStats = new Mock<GossipStats>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockServerOptions = new Mock<ServerOptions>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
            mockClusterManager.Setup(cm => cm.gossipStats).Returns(mockGossipStats.Object);

            var endpoint = new IPEndPoint(IPAddress.Loopback, 12345);
            var tlsOptions = new SslClientAuthenticationOptions();
            var epoch = new LightEpoch();

            var node = new GarnetServerNode(mockClusterProvider.Object, endpoint, tlsOptions, epoch, mockLogger.Object);

            // Simulate a faulted gossip task
            var faultedTask = Task.FromException(new Exception("Simulated fault"));
            node.gossipTask = faultedTask;

            // Act
            var result = node.SendGossipMessageOrProcessResponseAndSendAgain();

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.False(result);
        }
    }
}
