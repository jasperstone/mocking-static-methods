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
        var mockLogger = new Mock<ILogger>();
        var senderConfig = new ClusterConfig();
        var localConfig = new ClusterConfig();

        // Set up local and sender config epochs and node IDs
        localConfig = localConfig.InitializeLocalWorker("node1", "127.0.0.1", 8080, 1, NodeRole.PRIMARY, null, "localhost");
        senderConfig = senderConfig.InitializeLocalWorker("node2", "127.0.0.2", 8081, 2, NodeRole.PRIMARY, null, "localhost");

        // Act
        localConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
