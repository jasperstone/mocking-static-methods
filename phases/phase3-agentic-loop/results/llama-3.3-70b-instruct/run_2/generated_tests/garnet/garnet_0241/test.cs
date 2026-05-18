using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace GarnetTests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<Garnet.cluster.ReplicationManager>();
            replicationManagerMock.Setup(r => r.CannotStreamAOF).Returns(true);
            var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
            var replicationReplicaAofSync = new Garnet.cluster.ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object, clusterProviderMock.Object);

            // Act
            replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);

            // Assert
            loggerMock.Verify(l => l.LogError("Replica is recovering cannot sync AOF"), Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenNotReplica()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<Garnet.cluster.ReplicationManager>();
            replicationManagerMock.Setup(r => r.CannotStreamAOF).Returns(false);
            var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig.LocalNodeRole).Returns(Garnet.cluster.NodeRole.PRIMARY);
            var replicationReplicaAofSync = new Garnet.cluster.ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object, clusterProviderMock.Object);

            // Act
            replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);

            // Assert
            loggerMock.Verify(l => l.LogWarning("This node {nodeId} is not a replica", It.IsAny<long>()), Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenDivergentAOFStream()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<Garnet.cluster.ReplicationManager>();
            replicationManagerMock.Setup(r => r.CannotStreamAOF).Returns(false);
            var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig.LocalNodeRole).Returns(Garnet.cluster.NodeRole.REPLICA);
            var replicationReplicaAofSync = new Garnet.cluster.ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object, clusterProviderMock.Object);

            // Act
            replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);

            // Assert
            loggerMock.Verify(l => l.LogError("Divergent AOF Stream recordLength:{recordLength}; previousAddress:{previousAddress}; currentAddress:{currentAddress}; nextAddress:{nextAddress}; tailAddress:{tail}", It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()), Times.Once);
        }
    }
}
