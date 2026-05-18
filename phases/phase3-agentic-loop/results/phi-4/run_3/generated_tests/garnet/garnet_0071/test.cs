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
        public async Task LogWarning_ShouldBeCalled_WhenTaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);

            var node = new GarnetServerNode(clusterProviderMock.Object, new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 6379), null, new LightEpoch(), mockLogger.Object);

            var faultedTask = new TaskCompletionSource<object>();
            faultedTask.SetException(new Exception("Test exception"));

            // Act
            bool result = await node.TryGossipAsync(faultedTask.Task);

            // Assert
            mockLogger.Verify(
                l => l.LogWarning(
                    It.IsAny<Exception>(),
                    "GOSSIP round faulted",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.False(result);
        }
    }
}
