using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

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
            var senderConfigEpoch = 2L; // different epoch, no collision

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(localNodeId, "127.0.0.1", 7000, localConfigEpoch, NodeRole.PRIMARY, null, "localhost");

            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(senderNodeId, "127.0.0.2", 7001, senderConfigEpoch, NodeRole.PRIMARY, null, "localhost");

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
                .InitializeLocalWorker(nodeId, "127.0.0.1", 7000, configEpoch, NodeRole.PRIMARY, null, "localhost");

            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeId, "127.0.0.2", 7001, configEpoch, NodeRole.PRIMARY, null, "localhost");

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
            var nodeIdLocal = "nodeLocal";
            var nodeIdSender = "nodeSender";
            var configEpoch = 5L;

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeIdLocal, "10.0.0.1", 7000, configEpoch, NodeRole.PRIMARY, null, "localhost");

            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeIdSender, "10.0.0.2", 7001, configEpoch, NodeRole.PRIMARY, null, "localhost");

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            // It should log a warning with the expected message template and parameters
            loggerMock.Verify(
                x => x.LogWarning(
                    "Epoch Collision {localNodeConfigEpoch} <> {senderConfigEpoch} [{LocalNodeIp}:{LocalNodePort},{localNodeId}] [{senderIp}:{senderPort},{senderNodeId}]",
                    configEpoch,
                    configEpoch,
                    "10.0.0.1",
                    7000,
                    It.IsAny<string>(),
                    "10.0.0.2",
                    7001,
                    It.IsAny<string>()),
                Times.Once);

            // The returned config should have bumped the local node config epoch by 1
            Assert.NotSame(localConfig, result);
            Assert.Equal(configEpoch + 1, result.LocalNodeConfigEpoch);
        }

        [Fact]
        public void HandleConfigEpochCollision_NullLogger_DoesNotThrowAndBumpsEpoch()
        {
            // Arrange
            var nodeIdLocal = "nodeLocal";
            var nodeIdSender = "nodeSender";
            var configEpoch = 10L;

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeIdLocal, "192.168.1.1", 8000, configEpoch, NodeRole.PRIMARY, null, "localhost");

            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeIdSender, "192.168.1.2", 8001, configEpoch, NodeRole.PRIMARY, null, "localhost");

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, null);

            // Assert
            Assert.NotSame(localConfig, result);
            Assert.Equal(configEpoch + 1, result.LocalNodeConfigEpoch);
        }
    }
}
