using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_ReplicaIsRecovering_ThrowsGarnetException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();

            replicationManagerMock.Setup(rm => rm.CannotStreamAOF).Returns(true);

            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);

            var replicationReplicaAofSync = new ReplicationManager
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object
            };

            // Act & Assert
            var exception = Assert.Throws<GarnetException>(() => replicationReplicaAofSync.ProcessPrimaryStream(null, 0, 0, 0, 0));
            Assert.Equal("Replica is recovering cannot sync AOF", exception.Message);
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_NodeIsNotReplica_ThrowsGarnetException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var currentConfigMock = new Mock<ClusterConfig>();
            var replicationManagerMock = new Mock<IReplicationManager>();

            currentConfigMock.Setup(cc => cc.LocalNodeRole).Returns(NodeRole.PRIMARY);
            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);

            var replicationReplicaAofSync = new ReplicationManager
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object
            };

            // Act & Assert
            var exception = Assert.Throws<GarnetException>(() => replicationReplicaAofSync.ProcessPrimaryStream(null, 0, 0, 0, 0));
            Assert.Contains("is not a replica", exception.Message);
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
