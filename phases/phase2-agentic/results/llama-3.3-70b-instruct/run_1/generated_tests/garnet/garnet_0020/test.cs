using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_LogsWarningAndBumpsEpoch_WhenSenderConfigEpochMatchesLocalNodeConfigEpochAndSenderNodeIdIsGreater()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("localNodeId", "localIp", 1234, 1, NodeRole.PRIMARY, null, "localHostname");
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("senderNodeId", "senderIp", 5678, 1, NodeRole.PRIMARY, null, "senderHostname");

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.NotEqual(clusterConfig.LocalNodeConfigEpoch, result.LocalNodeConfigEpoch);
        }

        [Fact]
        public void HandleConfigEpochCollision_DoesNotLogWarningAndDoesNotBumpEpoch_WhenSenderConfigEpochDoesNotMatchLocalNodeConfigEpoch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("localNodeId", "localIp", 1234, 1, NodeRole.PRIMARY, null, "localHostname");
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("senderNodeId", "senderIp", 5678, 2, NodeRole.PRIMARY, null, "senderHostname");

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.Equal(clusterConfig.LocalNodeConfigEpoch, result.LocalNodeConfigEpoch);
        }

        [Fact]
        public void HandleConfigEpochCollision_DoesNotLogWarningAndDoesNotBumpEpoch_WhenSenderNodeIdIsLessThanOrEqualToLocalNodeId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("localNodeId", "localIp", 1234, 1, NodeRole.PRIMARY, null, "localHostname");
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("senderNodeId", "senderIp", 5678, 1, NodeRole.PRIMARY, null, "senderHostname");
            senderConfig.LocalNodeId = "localNodeId";

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.Equal(clusterConfig.LocalNodeConfigEpoch, result.LocalNodeConfigEpoch);
        }
    }
}
