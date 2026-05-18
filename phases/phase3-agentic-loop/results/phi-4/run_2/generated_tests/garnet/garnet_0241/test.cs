using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict);

            // Initialize ReplicationManager and set up the condition
            var replicationManagerMock = new Mock<ReplicationManager>(MockBehavior.Strict);
            replicationManagerMock.Setup(r => r.CannotStreamAOF).Returns(true);
            clusterProviderMock.Setup(c => c.InitializeReplicationManager()).Callback(() => c.Object.replicationManager = replicationManagerMock.Object);

            // Assuming ReplicationReplicaAofSync has a constructor that takes ClusterProvider and ILogger
            var replicationReplicaAofSync = new ReplicationReplicaAofSync(clusterProviderMock.Object, loggerMock.Object);

            // Act
            byte[] record = new byte[10];
            replicationReplicaAofSync.ProcessPrimaryStream(record, record.Length, 0, 0, 0);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
