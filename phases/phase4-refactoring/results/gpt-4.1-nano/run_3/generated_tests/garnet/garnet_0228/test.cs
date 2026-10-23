using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.test.cluster
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogError_When_AddressIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions
            {
                NodeId = 1,
                TryAddReplica = true,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            var config = new ClusterConfig
            {
                GetLocalNodePrimaryAddress = () => (null, -1)
            };

            var currentConfig = config;

            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfig);
            clusterProviderMock.Setup(cp => cp.clusterManager.TryAddReplicaAsync(It.IsAny<int>(), false, false, null))
                .ReturnsAsync((true, (ReadOnlyMemory<byte>)null));
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(new ReplicationManager());
            clusterProviderMock.Setup(cp => cp.GetIRSNetworkBufferSettings).Returns(() => new object());
            clusterProviderMock.Setup(cp => cp.GetNetworkPool).Returns(() => new object());

            var manager = new ReplicationManager
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                storeWrapper = storeWrapperMock.Object,
                ctsRepManager = new CancellationTokenSource(),
                IsRecovering = true
            };

            // Act
            var result = await manager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(ReplicationManager.TryReplicateDiskbasedSyncAsync)))),
                Times.Once);
            Assert.False(result.Success);
        }
    }
}
