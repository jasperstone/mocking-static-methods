using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ClusterConfigWrapper
{
    private readonly ClusterConfig _config;

    public ClusterConfigWrapper()
    {
        _config = new ClusterConfig();
    }

    public ClusterConfigWrapper InitializeLocalWorker(
        string nodeId,
        string address,
        int port,
        long configEpoch,
        NodeRole role,
        string replicaOfNodeId,
        string hostname)
    {
        _config.InitializeLocalWorker(nodeId, address, port, configEpoch, role, replicaOfNodeId, hostname);
        return this;
    }

    public ClusterConfig HandleConfigEpochCollision(ClusterConfig senderConfig, ILogger logger = null)
    {
        return _config.HandleConfigEpochCollision(senderConfig, logger);
    }
}

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_LogsWarning_WhenEpochsDifferAndSenderNodeIdIsGreater()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ClusterConfig>>();
        var localConfig = new ClusterConfigWrapper();
        var senderConfig = new ClusterConfigWrapper();

        localConfig.InitializeLocalWorker(
            "localNodeId",
            "127.0.0.1",
            8080,
            1,
            NodeRole.PRIMARY,
            null,
            "localhost"
        );

        senderConfig.InitializeLocalWorker(
            "senderNodeId",
            "127.0.0.2",
            8081,
            1,
            NodeRole.PRIMARY,
            null,
            "localhost"
        );

        // Act
        localConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

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
