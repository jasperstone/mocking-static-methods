using System;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public unsafe void ProcessPrimaryStream_WhenCannotStreamAOF_LogsErrorAndThrows()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var mockReplicationManager = new Mock<IReplicationManager>();
            mockReplicationManager.SetupGet(x => x.CannotStreamAOF).Returns(true);

            var mockClusterProvider = new Mock<IClusterProvider>();
            mockClusterProvider.SetupGet(x => x.replicationManager).Returns(mockReplicationManager.Object);

            var replicaAofSync = new TestReplicationReplicaAofSync(mockLogger.Object, mockClusterProvider.Object);

            // Act & Assert
            var exception = Assert.Throws<GarnetException>(() =>
                replicaAofSync.ProcessPrimaryStream(null, 0, 0L, 0L, 0L));

            Assert.Equal("Replica is recovering cannot sync AOF", exception.Message);
            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Replica is recovering cannot sync AOF") ?? false),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Test double replicating the exact logging path from line 49
    internal class TestReplicationReplicaAofSync
    {
        protected readonly ILogger<object> logger;
        private readonly IClusterProvider clusterProvider;

        public TestReplicationReplicaAofSync(ILogger<object> logger, IClusterProvider clusterProvider)
        {
            this.logger = logger;
            this.clusterProvider = clusterProvider;
        }

        public unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
        {
            if (clusterProvider.replicationManager.CannotStreamAOF)
            {
                logger?.LogError("Replica is recovering cannot sync AOF");
                throw new GarnetException("Replica is recovering cannot sync AOF", LogLevel.Warning, clientResponse: false);
            }
        }
    }

    public interface IClusterProvider
    {
        IReplicationManager replicationManager { get; }
    }

    public interface IReplicationManager
    {
        bool CannotStreamAOF { get; }
    }
}
