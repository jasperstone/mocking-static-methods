using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System;
using Garnet.client;
using Garnet.common;
using Garnet.server;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public void LogWarning_When_GossipTask_Faults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GarnetServerNode>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockGossipStats = new Mock<GossipStats>();
            var mockGarnetClient = new Mock<GarnetClient>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterManager.Setup(cm => cm.gossipStats).Returns(mockGossipStats.Object);

            var node = new GarnetServerNode(mockClusterProvider.Object, null, null, null, mockLogger.Object);

            var task = Task.FromException(new Exception("Test exception"));
            var gossipTaskField = typeof(GarnetServerNode).GetField("gossipTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gossipTaskField.SetValue(node, task);

            // Act
            var result = node.ProcessGossipResponse();

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result);
        }
    }
}
