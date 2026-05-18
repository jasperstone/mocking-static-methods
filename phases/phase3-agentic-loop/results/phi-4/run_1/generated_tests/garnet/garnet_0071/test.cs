using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public async Task LogWarning_ShouldBeCalled_WhenGossipTaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);

            var node = new GarnetServerNode(clusterProviderMock.Object, new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 6379), null, new LightEpoch(), mockLogger.Object);

            // Simulate a faulted task
            var faultedTask = new TaskCompletionSource<object>();
            faultedTask.SetException(new Exception("Gossip task faulted"));

            // Act
            var result = await node.GossipAsync(new byte[0]);

            // Assert
            mockLogger.Verify(
                l => l.LogWarning(It.IsAny<Exception>(), "GOSSIP round faulted"),
                Times.Once);
        }
    }
}
