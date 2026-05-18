using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public async Task GossipAsync_LogsWarning_WhenTaskFaults()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var garnetServerNode = new GarnetServerNode(clusterProviderMock.Object, null, null, null, loggerMock.Object);

            // Act
            var task = Task.FromException(new Exception("Test exception"));
            garnetServerNode.gossipTask = task;
            var result = await garnetServerNode.GossipAsync(new byte[0]);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
