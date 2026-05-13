using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Garnet.cluster;
using Garnet.client;
using Garnet.server;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_Should_LogError_When_LogErrorIsCalled()
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

            var replicationManager = new ReplicationManager
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                storeWrapper = storeWrapperMock.Object,
                ctsRepManager = new CancellationTokenSource()
            };

            // Setup clusterProvider mock
            var currentConfigMock = new Mock<IClusterConfig>();
            currentConfigMock.Setup(c => c.GetLocalNodePrimaryAddress())
                .Returns(("127.0.0.1", 1234));
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig)
                .Returns(currentConfigMock.Object);
            clusterProviderMock.Setup(c => c.clusterManager.TryAddReplicaAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .ReturnsAsync((true, (string)null));

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            Assert.True(result.Success);
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(replicationManager.TryReplicateDiskbasedSyncAsync)))),
                Times.Never);
        }

        [Fact]
        public async Task ReplicaSyncAttachTaskAsync_Should_LogError_When_AddressIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var replicationManager = new Mock<ReplicationManager> { CallBase = true };
            var options = new ReplicateSyncOptions
            {
                NodeId = 1,
                TryAddReplica = false,
                Force = false,
                UpgradeLock = false,
                Background = false
            };

            var currentConfigMock = new Mock<IClusterConfig>();
            currentConfigMock.Setup(c => c.GetLocalNodePrimaryAddress())
                .Returns((null, -1));
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig)
                .Returns(currentConfigMock.Object);
            clusterProviderMock.Setup(c => c.replicationManager).Returns(replicationManager.Object);
            clusterProviderMock.Setup(c => c.serverOptions).Returns(new ServerOptions());
            clusterProviderMock.Setup(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(c => c.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(c => c.TlsOptions).Returns((TlsOptions)null);
            clusterProviderMock.Setup(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(c => c.clusterManager.TryAddReplicaAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .ReturnsAsync((true, (string)null));

            // Act
            var result = await replicationManager.Object.TryReplicateDiskbasedSyncAsync(null, options);

            // Since address is null, LogError should be called
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(replicationManager.TryReplicateDiskbasedSyncAsync)))),
                Times.Once);
        }
    }
}
