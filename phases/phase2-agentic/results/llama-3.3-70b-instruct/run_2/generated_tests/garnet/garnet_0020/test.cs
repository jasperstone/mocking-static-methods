using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_LogsWarning_WhenEpochsAreEqual()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("node1", "127.0.0.1", 6379, 1, NodeRole.PRIMARY, null, "localhost");
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("node2", "127.0.0.1", 6380, 1, NodeRole.PRIMARY, null, "localhost");

            // Act
            clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void HandleConfigEpochCollision_DoesNotLogWarning_WhenEpochsAreNotEqual()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("node1", "127.0.0.1", 6379, 1, NodeRole.PRIMARY, null, "localhost");
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("node2", "127.0.0.1", 6380, 2, NodeRole.PRIMARY, null, "localhost");

            // Act
            clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_BumpsLocalNodeConfigEpoch_WhenEpochsAreEqualAndLocalNodeIdIsGreater()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("node1", "127.0.0.1", 6379, 1, NodeRole.PRIMARY, null, "localhost");
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("node2", "127.0.0.1", 6380, 1, NodeRole.PRIMARY, null, "localhost");

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            Assert.True(result.LocalNodeConfigEpoch > clusterConfig.LocalNodeConfigEpoch);
        }
    }
}
