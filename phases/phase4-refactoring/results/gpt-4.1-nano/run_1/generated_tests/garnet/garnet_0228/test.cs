using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Garnet.cluster;
using Garnet.server;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions
            {
                NodeId = "node1",
                TryAddReplica = true,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            var replicationManager = new ReplicationManager
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                storeWrapper = storeWrapperMock.Object,
                ctsRepManager = new CancellationTokenSource()
            };

            // Setup clusterProvider to throw in GetLocalNodePrimaryAddress
            var currentConfigMock = new Mock<IClusterConfig>();
            currentConfigMock.Setup(c => c.GetLocalNodePrimaryAddress())
                .Throws(new Exception("Simulated exception"));

            var clusterConfig = currentConfigMock.Object;

            var clusterManager = new Mock<IClusterManager>();
            clusterManager.Setup(c => c.CurrentConfig).Returns(clusterConfig);
            clusterProviderMock.Setup(c => c.clusterManager).Returns(clusterManager.Object);

            // Act
            await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(replicationManager.TryReplicateDiskbasedSyncAsync)))),
                Times.Once);
        }
    }
}
