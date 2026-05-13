using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_LogsWarning_WhenEpochsDifferAndSenderNodeIdIsGreater()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var localConfig = new ClusterConfig();
            var senderConfig = new ClusterConfig();

            localConfig = localConfig.InitializeLocalWorker("node1", "127.0.0.1", 6379, 1, NodeRole.PRIMARY, null, "localhost");
            senderConfig = senderConfig.InitializeLocalWorker("node2", "127.0.0.2", 6379, 1, NodeRole.PRIMARY, null, "localhost");

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }

        [Fact]
        public void HandleConfigEpochCollision_ReturnsBumpedConfig_WhenEpochsDifferAndSenderNodeIdIsGreater()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var localConfig = new ClusterConfig();
            var senderConfig = new ClusterConfig();

            localConfig = localConfig.InitializeLocalWorker("node1", "127.0.0.1", 6379, 1, NodeRole.PRIMARY, null, "localhost");
            senderConfig = senderConfig.InitializeLocalWorker("node2", "127.0.0.2", 6379, 1, NodeRole.PRIMARY, null, "localhost");

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

            // Assert
            Assert.Equal(localConfig.LocalNodeConfigEpoch + 1, result.LocalNodeConfigEpoch);
        }

        [Fact]
        public void HandleConfigEpochCollision_ReturnsSameConfig_WhenEpochsAreEqualAndSenderNodeIdIsGreater()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var localConfig = new ClusterConfig();
            var senderConfig = new ClusterConfig();

            localConfig = localConfig.InitializeLocalWorker("node1", "127.0.0.1", 6379, 1, NodeRole.PRIMARY, null, "localhost");
            senderConfig = senderConfig.InitializeLocalWorker("node2", "127.0.0.2", 6379, 1, NodeRole.PRIMARY, null, "localhost");

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

            // Assert
            Assert.Equal(localConfig, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_ReturnsSameConfig_WhenEpochsAreEqualAndSenderNodeIdIsSmaller()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var localConfig = new ClusterConfig();
            var senderConfig = new ClusterConfig();

            localConfig = localConfig.InitializeLocalWorker("node2", "127.0.0.2", 6379, 1, NodeRole.PRIMARY, null, "localhost");
            senderConfig = senderConfig.InitializeLocalWorker("node1", "127.0.0.1", 6379, 1, NodeRole.PRIMARY, null, "localhost");

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

            // Assert
            Assert.Equal(localConfig, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_ReturnsSameConfig_WhenEpochsDiffer()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var localConfig = new ClusterConfig();
            var senderConfig = new ClusterConfig();

            localConfig = localConfig.InitializeLocalWorker("node1", "127.0.0.1", 6379, 1, NodeRole.PRIMARY, null, "localhost");
            senderConfig = senderConfig.InitializeLocalWorker("node2", "127.0.0.2", 6379, 2, NodeRole.PRIMARY, null, "localhost");

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, mockLogger.Object);

            // Assert
            Assert.Equal(localConfig, result);
        }
    }
}
