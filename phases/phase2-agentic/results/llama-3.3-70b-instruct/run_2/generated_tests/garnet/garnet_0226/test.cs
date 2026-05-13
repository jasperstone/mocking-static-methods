using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogInformationForegroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager();
            var session = new ClusterSession();
            var options = new ReplicateSyncOptions("NodeId", false, false, true, false, false);

            // Act
            await replicationManager.TryReplicateDiskbasedSyncAsync(session, options);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Initiating foreground checkpoint retrieval"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogInformationBackgroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager();
            var session = new ClusterSession();
            var options = new ReplicateSyncOptions("NodeId", true, false, true, false, false);

            // Act
            await replicationManager.TryReplicateDiskbasedSyncAsync(session, options);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Initiating background checkpoint retrieval"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
