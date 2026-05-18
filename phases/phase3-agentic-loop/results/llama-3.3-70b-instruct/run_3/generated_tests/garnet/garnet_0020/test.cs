using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_LogsWarningAndBumpsEpoch_WhenSenderConfigEpochMatchesLocalNodeConfigEpoch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("localNodeId", "localIp", 1234, 1, NodeRole.PRIMARY, null, null);
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("senderNodeId", "senderIp", 5678, 1, NodeRole.PRIMARY, null, null);

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.NotEqual(clusterConfig.GetMaxConfigEpoch(), result.GetMaxConfigEpoch());
        }

        [Fact]
        public void HandleConfigEpochCollision_DoesNotLogWarningAndDoesNotBumpEpoch_WhenSenderConfigEpochDoesNotMatchLocalNodeConfigEpoch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("localNodeId", "localIp", 1234, 1, NodeRole.PRIMARY, null, null);
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("senderNodeId", "senderIp", 5678, 2, NodeRole.PRIMARY, null, null);

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.Equal(clusterConfig.GetMaxConfigEpoch(), result.GetMaxConfigEpoch());
        }

        [Fact]
        public void HandleConfigEpochCollision_DoesNotLogWarningAndDoesNotBumpEpoch_WhenSenderNodeIdIsLessThanLocalNodeId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig();
            clusterConfig.InitializeLocalWorker("localNodeId", "localIp", 1234, 1, NodeRole.PRIMARY, null, null);
            var senderConfig = new ClusterConfig();
            senderConfig.InitializeLocalWorker("senderNodeId", "senderIp", 5678, 1, NodeRole.PRIMARY, null, null);
            senderConfig.LocalNodeId = "abc";
            clusterConfig.LocalNodeId = "def";

            // Act
            var result = clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.Equal(clusterConfig.GetMaxConfigEpoch(), result.GetMaxConfigEpoch());
        }
    }
}
