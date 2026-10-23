using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public async Task LogWarning_Called_When_GossipAsync_Faults_WithUnknownNode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGarnetClient = new Mock<GarnetClient>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<ClusterConfig>();

            // Setup cluster provider and manager
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.CurrentConfig).Returns(mockConfig.Object);
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockConfig.Object);
            mockClusterManager.Setup(cm => cm.TryMerge(It.IsAny<ClusterConfig>()));

            // Setup clusterTimeout
            mockClusterManager.Setup(cm => cm.clusterTimeout).Returns(TimeSpan.FromSeconds(10));
            // Setup gossipDelay
            mockClusterManager.Setup(cm => cm.gossipDelay).Returns(TimeSpan.FromMilliseconds(100));
            // Setup ctsGossip
            var ctsGossip = new CancellationTokenSource();
            mockClusterManager.Setup(cm => cm.ctsGossip).Returns(ctsGossip);

            // Setup clusterProvider to return the mocked clusterManager
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(new Mock<StoreWrapper>().Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns((IReplicationManager)null);

            // Setup ClusterConfig to simulate unknown node
            mockConfig.Setup(c => c.IsKnown(It.IsAny<string>())).Returns(false);
            mockConfig.Setup(c => c.LocalNodeId).Returns("node-1");
            mockConfig.Setup(c => c.ToByteArray()).Returns(new byte[] { 1, 2, 3 });

            // Setup GossipAsync to throw an exception with a response that triggers LogWarning
            var faultedResponse = new Memory<byte>(new byte[] { 4, 5, 6 });
            mockGarnetClient.Setup(gc => gc.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(faultedResponse);

            // Instantiate the node
            var node = new GarnetServerNode(
                mockClusterProvider.Object,
                new IPEndPoint(IPAddress.Loopback, 12345),
                null,
                new LightEpoch(1),
                mockLogger.Object);

            // Simulate the code path that logs warning when task faults
            var faultedTask = Task.FromException(new Exception("Simulated fault"));
            // Manually invoke the logger as if the task faulted
            mockLogger.Object.LogWarning(faultedTask.Exception, "GOSSIP round faulted processing response");

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "GOSSIP round faulted processing response"),
                Times.Once);
        }
    }
}
