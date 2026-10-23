using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ClusterConfigPublicTests
    {
        [Fact]
        public void HandleConfigEpochCollision_NoCollision_ReturnsSameInstance()
        {
            var localNodeId = "localNode";
            var senderNodeId = "senderNode";

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(localNodeId, "127.0.0.1", 1234, 5, NodeRole.PRIMARY, null, "host1");
            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(senderNodeId, "127.0.0.2", 1235, 6, NodeRole.PRIMARY, null, "host2");

            var loggerMock = new Mock<ILogger>();

            var result = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            Assert.Same(localConfig, result);
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_SenderNodeIdLessOrEqualLocal_ReturnsSameInstance()
        {
            var nodeId = "node1";

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeId, "127.0.0.1", 1234, 5, NodeRole.PRIMARY, null, "host1");
            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(nodeId, "127.0.0.2", 1235, 5, NodeRole.PRIMARY, null, "host2");

            var loggerMock = new Mock<ILogger>();

            var result = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            Assert.Same(localConfig, result);
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_EpochCollision_LogsWarningAndBumpsEpoch()
        {
            var localNodeId = "node1";
            var senderNodeId = "node2";

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(localNodeId, "10.0.0.1", 1111, 5, NodeRole.PRIMARY, null, "host1");
            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(senderNodeId, "10.0.0.2", 2222, 5, NodeRole.PRIMARY, null, "host2");

            var loggerMock = new Mock<ILogger>();

            var result = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            loggerMock.Verify(l => l.LogWarning(
                "Epoch Collision {localNodeConfigEpoch} <> {senderConfigEpoch} [{LocalNodeIp}:{LocalNodePort},{localNodeId}] [{senderIp}:{senderPort},{senderNodeId}]",
                5L, 5L,
                "10.0.0.1", 1111, It.IsAny<string>(),
                "10.0.0.2", 2222, It.IsAny<string>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.NotSame(localConfig, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_NullLogger_DoesNotThrowAndBumpsEpoch()
        {
            var localNodeId = "node1";
            var senderNodeId = "node2";

            var localConfig = new ClusterConfig()
                .InitializeLocalWorker(localNodeId, "10.0.0.1", 1111, 5, NodeRole.PRIMARY, null, "host1");
            var senderConfig = new ClusterConfig()
                .InitializeLocalWorker(senderNodeId, "10.0.0.2", 2222, 5, NodeRole.PRIMARY, null, "host2");

            var result = localConfig.HandleConfigEpochCollision(senderConfig, null);

            Assert.NotNull(result);
            Assert.NotSame(localConfig, result);
        }
    }
}
