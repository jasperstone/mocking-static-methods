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
        public async Task LogWarningIsCalledWhenGossipTaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);

            var node = new GarnetServerNode(clusterProviderMock.Object, null, null, null, mockLogger.Object);
            var exception = new Exception("Test exception");

            // Simulate a faulted task
            var faultedTask = Task.FromException(exception);

            // Act
            // Simulate the method that handles the gossip task
            bool result = await node.HandleGossipTaskAsync(faultedTask);

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(It.IsAny<Exception>(), "GOSSIP round faulted"),
                Times.Once);

            Assert.False(result);
        }
    }
}
