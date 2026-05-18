using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_NoCollision_ReturnsOriginalConfig()
    {
        // Arrange
        var config = new ClusterConfig();
        var senderConfig = new ClusterConfig();
        senderConfig = config.Copy();
        senderConfig.workers[1].ConfigEpoch = 2;
        var loggerMock = new Mock<ILogger>();

        // Act
        var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void HandleConfigEpochCollision_Collision_LogsWarningAndBumpsEpoch()
    {
        // Arrange
        var config = new ClusterConfig();
        config.workers[1].ConfigEpoch = 1;
        var senderConfig = new ClusterConfig();
        senderConfig = config.Copy();
        senderConfig.workers[1].ConfigEpoch = 1;
        senderConfig.workers[1].Nodeid = "node2";
        var loggerMock = new Mock<ILogger>();
        var loggerLoggerMock = new Mock<ILogger<ClusterConfig>>();

        // Act
        var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        Assert.NotEqual(config.workers[1].ConfigEpoch, result.workers[1].ConfigEpoch);
    }

    [Fact]
    public void HandleConfigEpochCollision_Collision_LocalNodeIdLessThanSenderNodeId_ReturnsOriginalConfig()
    {
        // Arrange
        var config = new ClusterConfig();
        config.workers[1].ConfigEpoch = 1;
        config.workers[1].Nodeid = "node1";
        var senderConfig = new ClusterConfig();
        senderConfig = config.Copy();
        senderConfig.workers[1].ConfigEpoch = 1;
        senderConfig.workers[1].Nodeid = "node2";
        var loggerMock = new Mock<ILogger>();

        // Act
        var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void HandleConfigEpochCollision_Collision_LocalNodeIdGreaterThanSenderNodeId_LogsWarningAndBumpsEpoch()
    {
        // Arrange
        var config = new ClusterConfig();
        config.workers[1].ConfigEpoch = 1;
        config.workers[1].Nodeid = "node2";
        var senderConfig = new ClusterConfig();
        senderConfig = config.Copy();
        senderConfig.workers[1].ConfigEpoch = 1;
        senderConfig.workers[1].Nodeid = "node1";
        var loggerMock = new Mock<ILogger>();

        // Act
        var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        Assert.NotEqual(config.workers[1].ConfigEpoch, result.workers[1].ConfigEpoch);
    }
}
