using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task TakeOverAsPrimaryAsync_LogsWarning_WhenRecoveryLockCannotBeAcquired()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManagerMock = new Mock<IReplicationManager>();

            clusterProviderMock.Setup(c => c.replicationManager).Returns(replicationManagerMock.Object);
            replicationManagerMock.Setup(r => r.BeginRecovery(It.IsAny<RecoveryStatus>(), false)).Returns(false);

            var session = new FailoverSession(clusterProviderMock.Object, mockLogger.Object);

            // Act
            var result = await session.TakeOverAsPrimaryAsync();

            // Assert
            mockLogger.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s.Contains(nameof(session.TakeOverAsPrimaryAsync))),
                    It.Is<string>(s => s.Contains(CmdStrings.RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK.ToString())),
                    It.IsAny<object[]>()),
                Times.Once);

            Assert.False(result);
        }
    }
}
