using System;
using System.Text;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.test.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict, null, null, null, null);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

            var options = new ReplicateSyncOptions(
                NodeId: "node1",
                Background: false,
                Force: false,
                TryAddReplica: false,
                AllowReplicaResetOnFailure: false,
                UpgradeLock: false);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(null, options);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Initiating foreground checkpoint retrieval")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsBackgroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict, null, null, null, null);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

            var options = new ReplicateSyncOptions(
                NodeId: "node1",
                Background: true,
                Force: false,
                TryAddReplica: false,
                AllowReplicaResetOnFailure: false,
                UpgradeLock: false);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(null, options);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Initiating background checkpoint retrieval")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
