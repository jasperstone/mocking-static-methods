using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public async Task GossipAsync_ShouldLogWarning_WhenResponseFromUnknownNode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockGossipStats = new Mock<GossipStats>();
            var mockCurrentConfig = new Mock<ClusterConfig>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockGarnetClient = new Mock<GarnetClient>();

            // Setup minimal properties
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.gossipStats).Returns(mockGossipStats.Object);
            mockClusterProvider.Setup(cp => cp.CurrentConfig).Returns(mockCurrentConfig.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(new StoreWrapper());

            // Create the node
            var node = new GarnetServerNode(
                mockClusterProvider.Object,
                new IPEndPoint(IPAddress.Loopback, 12345),
                null,
                new LightEpoch(1),
                mockLogger.Object);

            // Use reflection to invoke the private method
            var methodInfo = typeof(GarnetServerNode).GetMethod("GossipAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Create a response with unknown node id
            var responseBytes = new byte[] { 1, 2, 3 };
            var responseMemory = new Memory<byte>(responseBytes);

            // Setup the mock gc to return the response
            var mockGc = new Mock<GarnetClient>();
            mockGc.Setup(g => g.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(responseMemory);

            // Replace the gc in the node
            typeof(GarnetServerNode).GetField("gc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(node, mockGc.Object);

            // Setup current config to not recognize the node
            mockCurrentConfig.Setup(c => c.IsKnown(It.IsAny<string>())).Returns(false);
            mockCurrentConfig.Setup(c => c.LocalNodeId).Returns("node1");
            mockCurrentConfig.Setup(c => c.ToByteArray()).Returns(new byte[] { 1, 2, 3 });
            mockCurrentConfig.Setup(c => c.FromByteArray(It.IsAny<byte[]>())).Returns(new ClusterConfig("node2"));

            // Act
            await (Task)methodInfo.Invoke(node, new object[] { new byte[] { 1, 2, 3 } });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received gossip from unknown node")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
