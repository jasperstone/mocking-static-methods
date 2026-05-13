using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions { Background = false };

            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterManagerMock.Setup(cm => cm.TryAddReplicaAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .ReturnsAsync((true, default(ReadOnlyMemory<byte>)));

            var replicationManager = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object);

            // Act
            await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Initiating foreground checkpoint retrieval"),
                Times.Once);
        }
    }
}
