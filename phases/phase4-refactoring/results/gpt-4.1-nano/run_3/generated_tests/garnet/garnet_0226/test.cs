using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_Log_Foreground_Message_When_Background_Is_False()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var replicationManager = new ReplicationManager
            {
                logger = mockLogger.Object
                // Additional dependencies can be mocked or set up here if needed
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
            mockLogger.Verify(
                x => x.LogInformation("Initiating foreground checkpoint retrieval"),
                Times.Once);
        }
    }
}
