using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_ShouldLogWarningAndBumpEpoch_WhenEpochsAreEqualAndSenderNodeIdIsGreater()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var localConfig = new ClusterConfig();
            var senderConfig = localConfig.Copy();

            // Set senderConfig's LocalNodeConfigEpoch to be same as local
            var senderNodeId = "node-2";
            var localNodeId = "node-1";

            senderConfig = senderConfig.InitializeLocalWorker(
                nodeId: senderNodeId,
                address: "127.0.0.1",
                port: 6379,
                configEpoch: localConfig.LocalNodeConfigEpoch,
                role: NodeRole.MASTER,
                replicaOfNodeId: null,
                hostname: "localhost"
            );

            // Act
            var result = localConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            // Verify that LogWarning was called
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Epoch Collision")),
                    It.Is<long>(l => l == localConfig.LocalNodeConfigEpoch),
                    It.Is<long>(l => l == senderConfig.LocalNodeConfigEpoch),
                    It.IsAny<string>(), // LocalNodeIp
                    It.IsAny<int>(),    // LocalNodePort
                    It.IsAny<string>(), // LocalNodeIdShort
                    It.IsAny<string>(), // senderIp
                    It.IsAny<int>(),    // senderPort
                    It.IsAny<string>()  // senderNodeIdShort
                ),
                Times.Once
            );

            // The result's LocalNodeConfigEpoch should be incremented
            Assert.True(result.LocalNodeConfigEpoch > localConfig.LocalNodeConfigEpoch);
        }
    }
}
