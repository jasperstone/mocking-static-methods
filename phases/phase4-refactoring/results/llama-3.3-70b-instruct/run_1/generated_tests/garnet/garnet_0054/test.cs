using Xunit;
using Moq;
using System.Threading.Tasks;
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
            clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(false);
            var replicaFailoverSession = new FailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Act
            await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(true);
            clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(false);
            var replicaFailoverSession = new FailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Act
            await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
