using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_LogsWarning_WhenCollisionOccurs()
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
    public void HandleConfigEpochCollision_BumpsLocalNodeConfigEpoch_WhenCollisionOccurs()
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

    [Fact]
    public void HandleConfigEpochCollision_DoesNotLogWarning_WhenNoCollisionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfig = new ClusterConfig();
        var senderConfig = new ClusterConfig();
        senderConfig.LocalNodeConfigEpoch = 1;

        // Act
        clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void HandleConfigEpochCollision_DoesNotBumpLocalNodeConfigEpoch_WhenNoCollisionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfig = new ClusterConfig();
        var senderConfig = new ClusterConfig();
        senderConfig.LocalNodeConfigEpoch = 1;

        // Act
        var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        Assert.Equal(clusterConfig.LocalNodeConfigEpoch, result.LocalNodeConfigEpoch);
    }
}
