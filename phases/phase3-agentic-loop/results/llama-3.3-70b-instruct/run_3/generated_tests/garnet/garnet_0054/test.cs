using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<Garnet.cluster.ReplicationManager>();
            replicationManagerMock.Setup(rm => rm.BeginRecovery(It.IsAny<Garnet.cluster.RecoveryStatus>(), It.IsAny<bool>())).Returns(false);
            var clusterManagerMock = new Mock<Garnet.cluster.ClusterManager>();
            var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);

            var replicaFailoverSession = new Garnet.cluster.FailoverSession(loggerMock.Object, clusterProviderMock.Object);

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
            var replicationManagerMock = new Mock<Garnet.cluster.ReplicationManager>();
            replicationManagerMock.Setup(rm => rm.BeginRecovery(It.IsAny<Garnet.cluster.RecoveryStatus>(), It.IsAny<bool>())).Returns(true);
            var clusterManagerMock = new Mock<Garnet.cluster.ClusterManager>();
            clusterManagerMock.Setup(cm => cm.TryTakeOverForPrimary()).Returns(false);
            var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);

            var replicaFailoverSession = new Garnet.cluster.FailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.False(result);
        }
    }
}
