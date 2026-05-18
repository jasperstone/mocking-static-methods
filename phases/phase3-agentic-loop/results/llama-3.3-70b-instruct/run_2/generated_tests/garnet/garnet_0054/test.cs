using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            replicationManagerMock.Setup(rm => rm.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(false);

            var replicaFailoverSession = new ReplicaFailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterManagerMock.Setup(cm => cm.TryTakeOverForPrimary()).Returns(false);

            var replicaFailoverSession = new ReplicaFailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenInitializeCheckpointStoreFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            replicationManagerMock.Setup(rm => rm.InitializeCheckpointStore()).Returns(false);

            var replicaFailoverSession = new ReplicaFailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.False(result);
        }
    }
}
