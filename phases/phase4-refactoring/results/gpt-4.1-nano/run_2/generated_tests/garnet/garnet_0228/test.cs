using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockSession = new Mock<ClusterSession>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockClusterConfig = new Mock<IClusterConfig>();
            var mockGcs = new Mock<GarnetClientSession>();

            // Setup dependencies
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockClusterConfig.Object);
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(new ReplicationManager());
            mockClusterProvider.Setup(cp => cp.GetGarnetClientSession(It.IsAny<EndPoint>(), It.IsAny<Func<BufferSettings>>(), It.IsAny<Func<NetworkPool>>(), It.IsAny<TlsClientOptions>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockGcs.Object);

            var manager = new ReplicationManager
            {
                logger = mockLogger.Object,
                clusterProvider = mockClusterProvider.Object,
                storeWrapper = mockStoreWrapper.Object,
                ctsRepManager = new CancellationTokenSource()
            };

            // Make TryAddReplicaAsync throw to simulate exception
            mockClusterManager.Setup(cm => cm.TryAddReplicaAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var options = new ReplicateSyncOptions
            {
                NodeId = "node1",
                TryAddReplica = true,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            // Act
            var result = await manager.TryReplicateDiskbasedSyncAsync(mockSession.Object, options);

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(manager.TryReplicateDiskbasedSyncAsync)))),
                Times.Once);
        }
    }
}
