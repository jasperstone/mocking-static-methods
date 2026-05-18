using Xunit;
using Moq;
using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster.Tests
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public void LogWarning_WhenTaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockGarnetClient = new Mock<GarnetClient>();
            var mockGossipStats = new Mock<GossipStats>();
            var mockCancellationTokenSource = new Mock<CancellationTokenSource>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterManager.Setup(cm => cm.gossipStats).Returns(mockGossipStats.Object);
            mockClusterManager.Setup(cm => cm.ctsGossip).Returns(mockCancellationTokenSource.Object);

            var node = new GarnetServerNode(
                mockClusterProvider.Object,
                new IPEndPoint(IPAddress.Loopback, 12345),
                new SslClientAuthenticationOptions(),
                new LightEpoch(),
                mockLogger.Object
            );

            var task = Task.FromException(new Exception("Test exception"));
            node.gossipTask = task;

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
