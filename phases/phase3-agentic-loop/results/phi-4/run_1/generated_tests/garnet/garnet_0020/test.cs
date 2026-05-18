using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ClusterConfigTests
{
    [Fact]
    public void HandleConfigEpochCollision_LogsWarningOnCollision()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterConfig = new ClusterConfig();
        var senderConfig = new ClusterConfig();

        // Set up the conditions for an epoch collision
        clusterConfig.InitializeLocalWorker("localNodeId", "127.0.0.1", 7000, 1, NodeRole.MASTER, null, "localhost");
        senderConfig.InitializeLocalWorker("senderNodeId", "127.0.0.2", 7001, 1, NodeRole.MASTER, null, "localhost");

        // Act
        var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(
                It.Is<string>(s => s.Contains("Epoch Collision")),
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Once);
    }
}
