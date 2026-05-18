using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Replication.ReplicaOps.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundCheckpointRetrieval_WhenForeground()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockSession = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions 
            { 
                Background = false, 
                UpgradeLock = false,
                TryAddReplica = false
            };

            mockClusterProvider.Setup(cp => cp.clusterManager.TryAddReplicaAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger<ReplicationManager>>()))
                              .ReturnsAsync((true, default(ReadOnlyMemory<byte>))));

            mockSession.Setup(s => s.UnsafeBumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            var replicationManager = new TestableReplicationManager(mockLogger.Object, mockClusterProvider.Object);
            
            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(mockSession.Object, options);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Initiating foreground checkpoint retrieval"),
                Times.Once);
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsBackgroundCheckpointRetrieval_WhenBackground()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockSession = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions 
            { 
                Background = true, 
                UpgradeLock = false,
                TryAddReplica = false
            };

            mockClusterProvider.Setup(cp => cp.clusterManager.TryAddReplicaAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger<ReplicationManager>>()))
                              .ReturnsAsync((true, default(ReadOnlyMemory<byte>))));

            mockSession.Setup(s => s.UnsafeBumpAndWaitForEpochTransitionAsync()).Returns(Task.CompletedTask);

            var replicationManager = new TestableReplicationManager(mockLogger.Object, mockClusterProvider.Object);
            
            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(mockSession.Object, options);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Initiating background checkpoint retrieval"),
                Times.Once);
        }

        private class TestableReplicationManager : ReplicationManager
        {
            public TestableReplicationManager(ILogger<ReplicationManager> logger, IClusterProvider clusterProvider) 
                : base(logger, clusterProvider)
            {
            }

            protected override async Task<string> ReplicaSyncAttachTaskAsync(bool downgradeLock, bool forceAsync)
            {
                if (forceAsync)
                {
                    await Task.Yield();
                }
                return null; // Success case
            }
        }
    }
}
