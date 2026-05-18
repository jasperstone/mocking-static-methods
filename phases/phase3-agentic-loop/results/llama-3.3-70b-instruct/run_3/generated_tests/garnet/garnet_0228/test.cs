using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void TryReplicateDiskbasedSyncAsync_LogsError_WhenPrimaryAddressIsNotAssigned()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
            var clusterManagerMock = new Mock<Garnet.cluster.ClusterManager>();
            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterManagerMock.SetupGet(cm => cm.CurrentConfig).Returns(new Garnet.cluster.ClusterConfig());

            var replicationManager = new Garnet.cluster.ReplicationManager();
            replicationManager.logger = loggerMock.Object;
            replicationManager.clusterProvider = clusterProviderMock.Object;

            // Act
            var result = replicationManager.TryReplicateDiskbasedSyncAsync(null, new Garnet.cluster.ReplicateSyncOptions());

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
