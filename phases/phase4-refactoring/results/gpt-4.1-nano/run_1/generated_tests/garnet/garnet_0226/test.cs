using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_ShouldLogForegroundMessage_WhenBackgroundIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var replicationManager = new ReplicationManager
            {
                logger = loggerMock.Object,
                clusterProvider = null // set to null or a mock if needed
            };

            var options = new ReplicateSyncOptions
            {
                NodeId = 1,
                TryAddReplica = false,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(null, options);

            // Assert
            Assert.True(result.Success);
            loggerMock.Verify(
                x => x.LogInformation("Initiating foreground checkpoint retrieval"),
                Times.Once);
        }
    }
}
