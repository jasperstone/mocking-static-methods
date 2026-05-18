using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncLogTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockReplicationManager.Setup(rm => rm.CannotStreamAOF).Returns(true);
            
            // Mock other dependencies minimally to reach the LogError line
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new Mock<IServerOptions>().Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new Mock<IClusterManager>().Object);
            
            var replicationManager = new TestReplicationManager(loggerMock.Object, mockClusterProvider.Object);

            fixed (byte[] dummy = new byte[1])
            {
                byte* recordPtr = dummy;

                // Act & Assert
                var exception = Assert.Throws<GarnetException>(
                    () => replicationManager.ProcessPrimaryStream(recordPtr, 0, 0L, 0L, 0L));

                Assert.Equal("Replica is recovering cannot sync AOF", exception.Message);

                // Verify the LogError extension method was called (verifies ILogger.Log with Error level)
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v?.ToString() == "Replica is recovering cannot sync AOF"),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }
    }

    // Test subclass that exposes the method and provides minimal dependencies
    public class TestReplicationManager : ReplicationManager
    {
        public TestReplicationManager(ILogger<ReplicationManager> logger, IClusterProvider clusterProvider)
        {
            // Minimal constructor - would need full deps in real scenario
        }

        // Expose the method publicly for testing
        public new unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, 
            long currentAddress, long nextAddress)
        {
            base.ProcessPrimaryStream(record, recordLength, previousAddress, currentAddress, nextAddress);
        }
    }
}
