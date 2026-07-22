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
            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.Setup(cp => cp.replicationManager.CannotStreamAOF).Returns(true);
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, clusterProviderMock.Object);

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
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenNodeIsNotReplica()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.Setup(cp => cp.replicationManager.CannotStreamAOF).Returns(false);
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig.LocalNodeRole).Returns(NodeRole.PRIMARY);
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, clusterProviderMock.Object);

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
