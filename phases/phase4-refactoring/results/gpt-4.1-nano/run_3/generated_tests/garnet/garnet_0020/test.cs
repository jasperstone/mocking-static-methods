using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_ShouldLogWarning_WhenEpochsMatchAndSenderNodeIdIsGreater()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var initialConfig = new ClusterConfig();
            // Set LocalNodeConfigEpoch and LocalNodeId for initialConfig
            var localEpoch = 5L;
            var localNodeId = "node-1";

            var configWithEpoch = initialConfig.InitializeLocalWorker(
                nodeId: localNodeId,
                address: "127.0.0.1",
                port: 6379,
                configEpoch: localEpoch,
                role: NodeRole.UNASSIGNED,
                replicaOfNodeId: null,
                hostname: "localhost"
            );

            // Create senderConfig with same epoch but higher node id
            var senderConfig = configWithEpoch.InitializeLocalWorker(
                nodeId: "node-2",
                address: "127.0.0.2",
                port: 6380,
                configEpoch: localEpoch,
                role: NodeRole.UNASSIGNED,
                replicaOfNodeId: null,
                hostname: "localhost"
            );

            // Act
            var resultConfig = configWithEpoch.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Epoch Collision")),
                    localEpoch,
                    localEpoch,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
