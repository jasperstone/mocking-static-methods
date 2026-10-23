using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public async Task LogWarning_WhenGossipTaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockGarnetClient = new Mock<GarnetClient>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockGossipStats = new Mock<GossipStats>();
            var mockConfig = new Mock<ClusterConfig>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockConfig.Object);
            mockClusterManager.Setup(cm => cm.gossipStats).Returns(mockGossipStats.Object);
            mockConfig.Setup(c => c.ToByteArray()).Returns(new byte[] { });

            var node = new GarnetServerNode(mockClusterProvider.Object, new IPEndPoint(IPAddress.Loopback, 12345), new SslClientAuthenticationOptions(), new LightEpoch(), mockLogger.Object);

            var task = Task.FromException(new Exception("Test exception"));
            node.gossipTask = task;

            // Act
            var result = node.SendGossipMessageOrProcessResponseAndSendAgain();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GOSSIP round faulted")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.False(result);
        }
    }
}
