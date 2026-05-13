using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cluster.Server.Failover.Tests
{
    public class LoggerExtensionsTests
    {
        private static readonly string IOErrorString =
            Encoding.ASCII.GetString(RespSrc.CmdStrings.RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK);

        [Fact]
        public void TakeOverAsPrimaryAsync_BeginRecoveryFails_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new Mock<IClusterProvider>();
            var replicationManagerMock = new Mock<IReplicationManager>();

            clusterProvider.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            replicationManagerMock.Setup(rm => rm.BeginRecovery(
                    RecoveryStatus.ClusterFailover,
                    It.IsAny<bool>()))
                .Returns(false);

            var session = new ReplicaFailoverSession(clusterProvider.Object, loggerMock.Object);

            // Act
            var result = session.TakeOverAsPrimaryAsync().GetAwaiter().GetResult();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString() == "TakeOverAsPrimaryAsync: {logMessage}"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static class RespSrc
        {
            public static class CmdStrings
            {
                public static readonly byte[] RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK =
                    Encoding.ASCII.GetBytes("ERR Cannot acquire recovery lock");
            }
        }

        private enum RecoveryStatus
        {
            ClusterFailover
        }

        private interface IClusterProvider
        {
            IReplicationManager replicationManager { get; }
        }

        private interface IReplicationManager
        {
            bool BeginRecovery(RecoveryStatus status, bool upgradeLock);
        }

        private class ReplicaFailoverSession
        {
            private readonly IClusterProvider clusterProvider;
            private readonly ILogger logger;

            public ReplicaFailoverSession(IClusterProvider clusterProvider, ILogger logger)
            {
                this.clusterProvider = clusterProvider;
                this.logger = logger;
            }

            public async Task<bool> TakeOverAsPrimaryAsync()
            {
                if (!clusterProvider.replicationManager.BeginRecovery(RecoveryStatus.ClusterFailover, false))
                {
                    logger?.LogWarning($"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}", Encoding.ASCII.GetString(RespSrc.CmdStrings.RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK));
                    return false;
                }
                return await Task.FromResult(true);
            }
        }
    }
}
