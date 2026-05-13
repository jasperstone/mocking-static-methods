using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationReplicaAofSync>>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.Setup(r => r.CannotStreamAOF).Returns(true);
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object);

            // Act
            try
            {
                replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);
            }
            catch (GarnetException)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError("Replica is recovering cannot sync AOF"), Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenNotReplica()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationReplicaAofSync>>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.Setup(r => r.CannotStreamAOF).Returns(false);
            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig.LocalNodeRole).Returns(NodeRole.PRIMARY);
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object, clusterProviderMock.Object);

            // Act
            try
            {
                replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);
            }
            catch (GarnetException)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning("This node {nodeId} is not a replica", It.IsAny<object>()), Times.Once);
        }
    }
}
