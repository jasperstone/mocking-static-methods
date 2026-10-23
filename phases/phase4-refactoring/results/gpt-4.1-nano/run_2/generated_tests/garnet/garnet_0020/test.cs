using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_ShouldLogWarningAndBumpEpoch_WhenCollisionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var originalConfig = new ClusterConfig();
            // Set local node epoch and id
            var localEpoch = 5L;
            var localNodeId = "node-1";

            var localConfig = originalConfig.InitializeLocalWorker(
                nodeId: localNodeId,
                address: "127.0.0.1",
                port: 6379,
                configEpoch: localEpoch,
                role: NodeRole.MASTER,
                replicaOfNodeId: null,
                hostname: "localhost"
            );

            // Create sender config with same epoch but higher node id
            var senderConfig = new ClusterConfig();
            senderConfig = senderConfig.InitializeLocalWorker(
                nodeId: "node-2",
                address: "127.0.0.2",
                port: 6380,
                configEpoch: localEpoch,
                role: NodeRole.MASTER,
                replicaOfNodeId: null,
                hostname: "localhost"
            );

            // Act
            var resultConfig = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            // Verify that LogWarning was called
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Epoch Collision")),
                    localEpoch,
                    localEpoch,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Once);

            // Verify that epoch was bumped
            Assert.Equal(localEpoch + 1, resultConfig.LocalNodeConfigEpoch);
        }
    }
}
