using Xunit;
using Moq;
using System;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationReplicaAofSync>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.Setup(c => c.replicationManager.CannotStreamAOF).Returns(true);
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, clusterProviderMock.Object);

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
        public void ProcessPrimaryStream_LogsError_WhenNodeIsNotReplica()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationReplicaAofSync>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.Setup(c => c.replicationManager.CannotStreamAOF).Returns(false);
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig.LocalNodeRole).Returns(NodeRole.PRIMARY);
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, clusterProviderMock.Object);

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
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
