using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
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
            clusterConfig = clusterConfig.InitializeLocalWorker("localNodeId", "127.0.0.1", 7000, 1, NodeRole.MASTER, null, "localhost");
            senderConfig = senderConfig.InitializeLocalWorker("senderNodeId", "127.0.0.2", 7001, 1, NodeRole.MASTER, null, "localhost");

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Epoch Collision")),
                    It.Is<long>(l => l == 1),
                    It.Is<long>(l => l == 1),
                    It.Is<string>(s => s == "127.0.0.1"),
                    It.Is<int>(i => i == 7000),
                    It.Is<string>(s => s == "localNodeId"),
                    It.Is<string>(s => s == "127.0.0.2"),
                    It.Is<int>(i => i == 7001),
                    It.Is<string>(s => s == "senderNodeId")),
                Times.Once);
        }
    }
}
