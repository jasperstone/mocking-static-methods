using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_LogsWarning_WhenEpochsDiffer()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var senderConfig = new ClusterConfig();
        var clusterConfig = new ClusterConfig();

        // Act
        clusterConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
