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
        public async Task LogWarningIsCalled_WhenGossipTaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);

            var node = new GarnetServerNode(clusterProviderMock.Object, null, null, null, mockLogger.Object);
            var exception = new Exception("Test exception");

            // Simulate a faulted task
            var faultedTask = new TaskCompletionSource<object>();
            faultedTask.SetException(exception);
            node.gossipTask = faultedTask.Task;

            // Act
            bool result = await node.TryGossipAsync();

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(It.IsAny<Exception>(), "GOSSIP round faulted"),
                Times.Once);
            Assert.False(result);
        }
    }
}
