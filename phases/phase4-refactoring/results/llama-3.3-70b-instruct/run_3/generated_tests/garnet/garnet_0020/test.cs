using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_LogsWarningAndBumpsEpoch_WhenCollisionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            var senderConfig = new ClusterConfig();

            clusterConfig.InitializeLocalWorker(
                "localNodeId",
                "localAddress",
                1234,
                1,
                NodeRole.PRIMARY,
                null,
                "localHostname");

            senderConfig.InitializeLocalWorker(
                "senderNodeId",
                "senderAddress",
                5678,
                1,
                NodeRole.PRIMARY,
                null,
                "senderHostname");

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.NotEqual(clusterConfig.LocalNodeConfigEpoch, result.LocalNodeConfigEpoch);
        }

        [Fact]
        public void HandleConfigEpochCollision_DoesNotLogWarningAndDoesNotBumpEpoch_WhenNoCollisionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            var senderConfig = new ClusterConfig();

            clusterConfig.InitializeLocalWorker(
                "localNodeId",
                "localAddress",
                1234,
                1,
                NodeRole.PRIMARY,
                null,
                "localHostname");

            senderConfig.InitializeLocalWorker(
                "senderNodeId",
                "senderAddress",
                5678,
                2,
                NodeRole.PRIMARY,
                null,
                "senderHostname");

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.Equal(clusterConfig.LocalNodeConfigEpoch, result.LocalNodeConfigEpoch);
        }
    }
}
