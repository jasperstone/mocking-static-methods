using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using System;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_ReplicaIsRecovering_LogsErrorAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationManagerMock = new Mock<ReplicationManager>(clusterProviderMock.Object, loggerMock.Object);

            replicationManagerMock.Setup(rm => rm.CannotStreamAOF).Returns(true);

            var replicaAofSync = new ReplicationReplicaAofSync(clusterProviderMock.Object, storeWrapperMock.Object, loggerMock.Object);

            // Act & Assert
            var exception = Assert.Throws<GarnetException>(() => replicaAofSync.ProcessPrimaryStream(null, 0, 0, 0, 0));
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Equal("Replica is recovering cannot sync AOF", exception.Message);
        }
    }
}
