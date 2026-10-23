using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_LogsWarning_WhenEpochsMatch()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfig = new ClusterConfig();
        var senderConfig = new ClusterConfig();

        // Act
        clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void HandleConfigEpochCollision_DoesNotLogWarning_WhenEpochsDoNotMatch()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfig = new ClusterConfig();
        var senderConfig = new ClusterConfig();

        // Act
        clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void HandleConfigEpochCollision_BumpsLocalNodeConfigEpoch_WhenEpochsMatchAndNodeIdIsGreater()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfig = new ClusterConfig();
        var senderConfig = new ClusterConfig();

        // Act
        var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        Assert.NotEqual(clusterConfig.LocalNodeConfigEpoch, result.LocalNodeConfigEpoch);
    }
}
