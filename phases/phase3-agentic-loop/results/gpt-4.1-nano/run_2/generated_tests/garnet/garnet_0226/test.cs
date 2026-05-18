using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogForegroundRetrieval_When_OptionsBackgroundIsFalse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockSession = new Mock<ClusterSession>();

            // Setup cluster provider to return a cluster manager
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            // Setup cluster manager to simulate successful TryAddReplicaAsync
            mockClusterManager.Setup(cm => cm.TryAddReplicaAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .ReturnsAsync((true, (ReadOnlyMemory<byte>)Array.Empty<byte>()));

            // Setup cluster provider to return a current config with a primary address
            var mockConfig = new Mock<IClusterConfig>();
            mockConfig.Setup(c => c.GetLocalNodePrimaryAddress()).Returns(("127.0.0.1", 12345));
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockConfig.Object);

            // Create an instance of the class under test
            var replicationManager = new ReplicationManager
            {
                logger = mockLogger.Object,
                clusterProvider = mockClusterProvider.Object,
                storeWrapper = new Mock<IStoreWrapper>().Object,
                ctsRepManager = new CancellationTokenSource(),
                IsRecovering = true
            };

            var options = new ReplicateSyncOptions
            {
                NodeId = "node1",
                TryAddReplica = true,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(mockSession.Object, options);

            // Assert
            Assert.True(result.Success);
            mockLogger.Verify(logger => logger.LogInformation("Initiating foreground checkpoint retrieval"), Times.Once);
        }
    }
}
