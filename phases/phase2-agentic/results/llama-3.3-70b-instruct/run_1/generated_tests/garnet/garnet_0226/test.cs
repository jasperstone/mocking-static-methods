using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster.Server.Replication;

namespace GarnetTests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogInformationForegroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions("NodeId", false, false, true, false, false);

            // Act
            var replicaReceiveCheckpoint = new ReplicaReceiveCheckpoint(loggerMock.Object, clusterProviderMock.Object);
            await replicaReceiveCheckpoint.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(l => l.Log(It.Is<LogLevel>(ll => ll == LogLevel.Information), It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString() == "Initiating foreground checkpoint retrieval"), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogInformationBackgroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions("NodeId", true, false, true, false, false);

            // Act
            var replicaReceiveCheckpoint = new ReplicaReceiveCheckpoint(loggerMock.Object, clusterProviderMock.Object);
            await replicaReceiveCheckpoint.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(l => l.Log(It.Is<LogLevel>(ll => ll == LogLevel.Information), It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString() == "Initiating background checkpoint retrieval"), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
