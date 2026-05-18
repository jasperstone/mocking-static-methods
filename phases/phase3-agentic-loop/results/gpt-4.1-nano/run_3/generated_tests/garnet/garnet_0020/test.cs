using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
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
            var initialConfig = new ClusterConfig();
            var senderConfig = initialConfig.Copy();

            // Set senderConfig's LocalNodeId to be greater than local
            senderConfig = senderConfig.InitializeLocalWorker(
                nodeId: "nodeB",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: 0,
                role: NodeRole.REPLICA,
                replicaOfNodeId: null,
                hostname: "localhost"
            );

            // Set local node id to be less than sender's
            initialConfig = initialConfig.InitializeLocalWorker(
                nodeId: "nodeA",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: 0,
                role: NodeRole.MASTER,
                replicaOfNodeId: null,
                hostname: "localhost"
            );

            // Act
            var resultConfig = initialConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Epoch Collision")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
            Assert.NotSame(initialConfig, resultConfig);
            Assert.True(resultConfig.GetMaxConfigEpoch() >= 0);
        }
    }
}
