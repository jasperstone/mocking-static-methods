using Xunit;
using Moq;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            replicationManagerMock.Setup(r => r.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(false);
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(c => c.replicationManager).Returns(replicationManagerMock.Object);
            var replicaFailoverSession = new FailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Act
            var result = await replicaFailoverSession.TakeOverAsPrimaryAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.False(result);
        }
    }
}
