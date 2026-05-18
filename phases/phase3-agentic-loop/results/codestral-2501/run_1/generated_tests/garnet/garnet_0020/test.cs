using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_LogsWarning_WhenEpochsDifferAndSenderNodeIdIsGreater()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ClusterConfig>>();
        var senderConfig = new ClusterConfig();
        var clusterConfig = new ClusterConfig();

        // Set up the senderConfig with a higher node ID and different config epoch
        senderConfig = senderConfig.InitializeLocalWorker(
            "node2",
            "127.0.0.2",
            1234,
            2,
            NodeRole.PRIMARY,
            null,
            "localhost"
        );

        // Set up the clusterConfig with a lower node ID and different config epoch
        clusterConfig = clusterConfig.InitializeLocalWorker(
            "node1",
            "127.0.0.1",
            1234,
            1,
            NodeRole.PRIMARY,
            null,
            "localhost"
        );

        // Act
        clusterConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}
