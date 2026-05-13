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
            var mockClusterManager = new Mock<ClusterManager>();
            var mockGossipStats = new Mock<GossipStats>();
            var mockGarnetClient = new Mock<GarnetClient>();
            var mockTask = new Mock<Task>();

            mockTask.Setup(t => t.Status).Returns(TaskStatus.Faulted);
            mockTask.Setup(t => t.Exception).Returns(new Exception("Test exception"));

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterManager.Setup(cm => cm.gossipStats).Returns(mockGossipStats.Object);

            var node = new GarnetServerNode(
                mockClusterProvider.Object,
                new IPEndPoint(IPAddress.Loopback, 12345),
                new SslClientAuthenticationOptions(),
                new LightEpoch(),
                mockLogger.Object
            );

            // Act
            var result = node.SendGossipMessage(mockTask.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.False(result);
        }
    }
}
