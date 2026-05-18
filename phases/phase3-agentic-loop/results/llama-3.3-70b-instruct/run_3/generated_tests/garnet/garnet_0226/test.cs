using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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
            replicationManager.logger = loggerMock.Object;
            var session = new ClusterSession();
            var options = new ReplicateSyncOptions { NodeId = "nodeId", Background = false };

            // Act
            await replicationManager.TryReplicateDiskbasedSyncAsync(session, options);

            // Assert
            loggerMock.Verify(l => l.Log(It.Is<LogLevel>(ll => ll == LogLevel.Information), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogInformationBackgroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager();
            replicationManager.logger = loggerMock.Object;
            var session = new ClusterSession();
            var options = new ReplicateSyncOptions { NodeId = "nodeId", Background = true };

            // Act
            await replicationManager.TryReplicateDiskbasedSyncAsync(session, options);

            // Assert
            loggerMock.Verify(l => l.Log(It.Is<LogLevel>(ll => ll == LogLevel.Information), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}
