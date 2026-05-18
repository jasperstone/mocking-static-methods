using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicationReplicaAofSyncTests
{
    [Fact]
    public void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManagerMock = new Mock<Garnet.cluster.ReplicationManager>();
        replicationManagerMock.Setup(rm => rm.CannotStreamAOF).Returns(true);
        var replicationReplicaAofSync = new Garnet.cluster.ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object);

        // Act
        replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);

        // Assert
        loggerMock.Verify(l => l.LogError("Replica is recovering cannot sync AOF"), Times.Once);
    }

    [Fact]
    public void ProcessPrimaryStream_LogsError_WhenNodeIsNotReplica()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManagerMock = new Mock<Garnet.cluster.ReplicationManager>();
        replicationManagerMock.Setup(rm => rm.CannotStreamAOF).Returns(false);
        var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
        clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig.LocalNodeRole).Returns(Garnet.cluster.NodeRole.PRIMARY);
        var replicationReplicaAofSync = new Garnet.cluster.ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object, clusterProviderMock.Object);

        // Act
        replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);

        // Assert
        loggerMock.Verify(l => l.LogWarning("This node {nodeId} is not a replica", It.IsAny<string>()), Times.Once);
    }
}
