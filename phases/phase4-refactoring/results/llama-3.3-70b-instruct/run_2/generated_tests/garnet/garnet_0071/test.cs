using Xunit;
using Moq;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

public class GarnetServerNodeTests
{
    [Fact]
    public async Task LogWarning_Called_When_GossipTask_Faults()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var garnetServerNode = new GarnetServerNode(clusterProviderMock.Object, null, null, null, loggerMock.Object);

        // Act
        garnetServerNode.gossipTask = Task.FromException(new Exception("Test exception"));
        await garnetServerNode.GossipAsync(new byte[0]);

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
