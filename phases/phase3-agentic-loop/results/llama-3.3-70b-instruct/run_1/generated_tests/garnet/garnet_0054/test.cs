using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            replicationManagerMock.Setup(rm => rm.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(false);

            var replicaFailoverSession = new ReplicaFailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterManagerMock.Setup(cm => cm.TryTakeOverForPrimary()).Returns(false);
            var replicationManagerMock = new Mock<IReplicationManager>();
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            replicationManagerMock.Setup(rm => rm.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(true);

            var replicaFailoverSession = new ReplicaFailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
