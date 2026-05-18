using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Garnet.cluster.Tests
{
    public class ClusterConfigLoggerExtensionsTests
    {
        [Fact]
        public void HandleConfigEpochCollision_CollisionWithHigherSenderNodeId_LogsWarningAndBumpsEpoch()
        {
            // Since ClusterConfig is internal, test the logging behavior indirectly
            // by verifying the control flow that triggers the LogWarning call on line 1508
            
            // Create configs where collision occurs and senderNodeId > localNodeId
            // This exercises the exact code path: same epoch -> senderNodeId.CompareTo(localNodeId) > 0 -> LogWarning -> BumpLocalNodeConfigEpoch()
            
            var config = new ClusterConfig();
            // Initialize local node with nodeId "local-123", epoch 10
            config = config.InitializeLocalWorker("local-123", "192.168.1.1", 6379, 10, NodeRole.PRIMARY, null, "localhost");
            
            var senderConfig = new ClusterConfig();
            // Initialize sender with higher nodeId "sender-456", same epoch 10
            senderConfig = senderConfig.InitializeLocalWorker("sender-456", "192.168.1.2", 6380, 10, NodeRole.PRIMARY, null, "senderhost");

            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            // Act - This will hit the LogWarning line 1508
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert - Verify LogWarning was called (using low-level Log verification)
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("Epoch Collision 10 <> 10") &&
                        v.ToString().Contains("192.168.1.1:6379") &&
                        v.ToString().Contains("local-123") &&
                        v.ToString().Contains("192.168.1.2:6380") &&
                        v.ToString().Contains("sender-456")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify the method returned a new config with bumped epoch
            Assert.NotSame(config, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_DifferentEpochs_DoesNotLog()
        {
            var config = new ClusterConfig();
            config = config.InitializeLocalWorker("local-123", "192.168.1.1", 6379, 10, NodeRole.PRIMARY, null, "localhost");
            
            var senderConfig = new ClusterConfig();
            senderConfig = senderConfig.InitializeLocalWorker("sender-456", "192.168.1.2", 6380, 11, NodeRole.PRIMARY, null, "senderhost"); // different epoch

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert - No logging (early return before line 1508)
            loggerMock.Verify(
                l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_SenderNodeIdLower_DoesNotLog()
        {
            var config = new ClusterConfig();
            config = config.InitializeLocalWorker("local-456", "192.168.1.1", 6379, 10, NodeRole.PRIMARY, null, "localhost"); // higher nodeId
            
            var senderConfig = new ClusterConfig();
            senderConfig = senderConfig.InitializeLocalWorker("sender-123", "192.168.1.2", 6380, 10, NodeRole.PRIMARY, null, "senderhost"); // lower nodeId, same epoch

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert - No logging (senderNodeId.CompareTo(localNodeId) <= 0 before line 1508)
            loggerMock.Verify(
                l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_NullLogger_DoesNotCrash()
        {
            var config = new ClusterConfig();
            config = config.InitializeLocalWorker("local-123", "192.168.1.1", 6379, 10, NodeRole.PRIMARY, null, "localhost");
            
            var senderConfig = new ClusterConfig();
            senderConfig = senderConfig.InitializeLocalWorker("sender-456", "192.168.1.2", 6380, 10, NodeRole.PRIMARY, null, "senderhost");

            // Act - null logger triggers logger?.LogWarning (safe null-conditional)
            var result = config.HandleConfigEpochCollision(senderConfig, null);

            // Assert - No exception thrown
            Assert.NotNull(result);
        }
    }
}
