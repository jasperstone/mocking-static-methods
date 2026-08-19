using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_ReplicaIsRecovering_LogsErrorAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();

            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);

            replicationManagerMock.Setup(rm => rm.CannotStreamAOF).Returns(true);

            var replicationReplicaAofSync = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object);

            // Act & Assert
            var exception = Assert.Throws<GarnetException>(() => replicationReplicaAofSync.ProcessPrimaryStream(null, 0, 0, 0, 0));
            Assert.Equal("Replica is recovering cannot sync AOF", exception.Message);
            loggerMock.Verify(
                x => x.LogError("Replica is recovering cannot sync AOF"),
                Times.Once);
        }
    }
}
