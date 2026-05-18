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
        public void ProcessPrimaryStream_ReplicaIsRecovering_LogsErrorAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationReplicaAofSync>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationManagerMock = new Mock<ReplicationManager>();

            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            replicationManagerMock.Setup(rm => rm.CannotStreamAOF).Returns(true);

            var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object, clusterProviderMock.Object, storeWrapperMock.Object);

            // Act
            Action act = () => replicationReplicaAofSync.ProcessPrimaryStream(IntPtr.Zero, 0, 0, 0, 0);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);

            var exception = Assert.Throws<GarnetException>(act);
            Assert.Equal("Replica is recovering cannot sync AOF", exception.Message);
        }
    }

    // Mock GarnetException class for testing purposes
    public class GarnetException : Exception
    {
        public GarnetException(string message, LogLevel logLevel, bool clientResponse) : base(message)
        {
        }
    }
}
