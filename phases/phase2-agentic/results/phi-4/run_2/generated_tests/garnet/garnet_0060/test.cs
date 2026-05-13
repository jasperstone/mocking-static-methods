using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReceivedFromUnknownNode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockClient = new Mock<IGarnetClient>();

            var oldConfig = new ClusterConfig
            {
                LocalNodePrimaryId = "primaryId"
            };

            var newConfig = new ClusterConfig
            {
                LocalNodePrimaryId = "newPrimaryId"
            };

            mockClusterProvider.Setup(p => p.clusterManager).Returns(mockClusterManager.Object);
            mockClusterManager.Setup(m => m.CurrentConfig).Returns(newConfig);
            mockClusterManager.Setup(m => m.TryMerge(It.IsAny<ClusterConfig>())).Returns(false);

            var session = new ReplicaFailoverSession(mockClusterProvider.Object, mockLogger.Object, oldConfig, newConfig);

            // Act
            await session.BroadcastConfigAndRequestAttachAsync("unknownNodeId", new byte[0]);

            // Assert
            mockLogger.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s == "Received gossip from unknown node: {node-id}"),
                    It.Is<string>(s => s == "unknownNodeId")),
                Times.Once);
        }
    }
}
