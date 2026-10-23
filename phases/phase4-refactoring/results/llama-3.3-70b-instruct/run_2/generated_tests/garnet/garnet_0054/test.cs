using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster.Server.Failover;

namespace garnet
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict, new object[] { new StoreWrapper() });
            clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(false);
            var replicaFailoverSession = new ReplicaFailoverSession(loggerMock.Object, clusterProviderMock.Object);
            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();
            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict, new object[] { new StoreWrapper() });
            clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(true);
            clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(false);
            var replicaFailoverSession = new ReplicaFailoverSession(loggerMock.Object, clusterProviderMock.Object);
            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();
            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }
    }
}
