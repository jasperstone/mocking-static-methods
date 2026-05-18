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
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager();

            // Act
            try
            {
                replicationManager.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);
            }
            catch (Exception)
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
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager();

            // Act
            try
            {
                replicationManager.ProcessPrimaryStream(new byte[0], 0, 0, 0, 0);
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning("This node {nodeId} is not a replica", It.IsAny<string>()), Times.Once);
        }
    }
}
