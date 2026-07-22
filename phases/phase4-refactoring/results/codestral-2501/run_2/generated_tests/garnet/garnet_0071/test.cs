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
        public void LogWarning_WhenTaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockGarnetClient = new Mock<GarnetClient>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockGossipStats = new Mock<GossipStats>();
            var mockEpoch = new Mock<LightEpoch>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterManager.Setup(cm => cm.gossipStats).Returns(mockGossipStats.Object);
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig());

            var node = new GarnetServerNode(
                mockClusterProvider.Object,
                new IPEndPoint(IPAddress.Loopback, 12345),
                new SslClientAuthenticationOptions(),
                mockEpoch.Object,
                mockLogger.Object
            );

            var task = Task.FromException(new Exception("Test exception"));

            // Act
            node.ProcessGossipTask(task);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
