using Xunit;
using Moq;
using System;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            replicationManagerMock.Setup(rm => rm.CannotStreamAOF).Returns(true);
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object);

            // Act
            try
            {
                replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError("Replica is recovering cannot sync AOF"), Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenNodeIsNotReplica()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            replicationManagerMock.Setup(rm => rm.CannotStreamAOF).Returns(false);
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig.LocalNodeRole).Returns(NodeRole.PRIMARY);
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, replicationManagerMock.Object, clusterProviderMock.Object);

            // Act
            try
            {
                replicationReplicaAofSync.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
