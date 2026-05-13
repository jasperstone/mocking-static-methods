using System;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_NoCollision_ReturnsSameInstance()
        {
            // Arrange
            var localNodeId = "localNode";
            var senderNodeId = "senderNode";
            var localConfigEpoch = 1L;
            var senderConfigEpoch = 2L; // Different epoch, no collision

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(localNodeId, "127.0.0.1", 7000, localConfigEpoch, NodeRole.PRIMARY, null, null);

            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(senderNodeId, "127.0.0.2", 7001, senderConfigEpoch, NodeRole.PRIMARY, null, null);

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            Assert.Same(localConfig, result);
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_SenderNodeIdLessOrEqualLocal_ReturnsSameInstance()
        {
            // Arrange
            var nodeId = "nodeA";
            var configEpoch = 1L;

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeId, "127.0.0.1", 7000, configEpoch, NodeRole.PRIMARY, null, null);

            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeId, "127.0.0.2", 7001, configEpoch, NodeRole.PRIMARY, null, null);

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            Assert.Same(localConfig, result);
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_EpochCollision_LogsWarningAndBumpsEpoch()
        {
            // Arrange
            var localNodeId = "nodeA";
            var senderNodeId = "nodeB"; // senderNodeId > localNodeId to trigger logging and bump
            var configEpoch = 1L;

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(localNodeId, "10.0.0.1", 7000, configEpoch, NodeRole.PRIMARY, null, "localHost");

            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(senderNodeId, "10.0.0.2", 7001, configEpoch, NodeRole.PRIMARY, null, "senderHost");

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            // It should log a warning once
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Epoch Collision")),
                    configEpoch,
                    configEpoch,
                    "10.0.0.1",
                    7000,
                    It.IsAny<string>(),
                    "10.0.0.2",
                    7001,
                    It.IsAny<string>()),
                Times.Once);

            // The returned config should have bumped local node config epoch by 1
            Assert.Equal(configEpoch + 1, result.LocalNodeConfigEpoch);
            Assert.Equal(localConfig.NumWorkers, result.NumWorkers);
            Assert.Equal(localConfig.LocalNodeId, result.LocalNodeId);
        }

        [Fact]
        public void HandleConfigEpochCollision_NullLogger_DoesNotThrowAndBumpsEpoch()
        {
            // Arrange
            var localNodeId = "nodeA";
            var senderNodeId = "nodeB"; // senderNodeId > localNodeId to trigger bump
            var configEpoch = 1L;

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(localNodeId, "10.0.0.1", 7000, configEpoch, NodeRole.PRIMARY, null, "localHost");

            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(senderNodeId, "10.0.0.2", 7001, configEpoch, NodeRole.PRIMARY, null, "senderHost");

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, null);

            // Assert
            Assert.Equal(configEpoch + 1, result.LocalNodeConfigEpoch);
        }
    }
}
