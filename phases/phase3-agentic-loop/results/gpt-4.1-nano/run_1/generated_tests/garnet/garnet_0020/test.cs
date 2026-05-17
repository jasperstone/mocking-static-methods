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

            // Create a sender config with same epoch and higher node id
            var senderConfig = initialConfig.Copy();

            // Initialize senderConfig with a higher node id
            senderConfig = senderConfig.InitializeLocalWorker(
                nodeId: "nodeB",
                address: "127.0.0.1",
                port: 6379,
                configEpoch: initialConfig.GetMaxConfigEpoch(),
                role: NodeRole.REPLICA,
                replicaOfNodeId: null,
                hostname: "localhost"
            );

            // Act
            var result = initialConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Epoch Collision")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
            Assert.NotSame(initialConfig, result);
        }
    }
}
