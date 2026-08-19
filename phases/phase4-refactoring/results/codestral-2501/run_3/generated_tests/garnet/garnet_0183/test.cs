using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Garnet.common;
using Garnet.server;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsCheckpointSearchCompleted()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockReplicaSyncMetadata = new Mock<SyncMetadata>();
            var mockReplicaCheckpointEntry = new Mock<CheckpointEntry>();
            var mockReplicaNodeId = "replicaNodeId";
            var mockReplicaAssignedPrimaryId = "replicaAssignedPrimaryId";
            var mockReplicaAofBeginAddress = 0L;
            var mockReplicaAofTailAddress = 0L;

            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(new Mock<IReplicationLogCheckpointManager>().Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(new Mock<IReplicationLogCheckpointManager>().Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig.GetWorkerAddressFromNodeId(mockReplicaNodeId)).Returns((IPAddress.Parse("127.0.0.1"), 12345));

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                mockReplicaSyncMetadata.Object,
                CancellationToken.None,
                mockReplicaNodeId,
                mockReplicaAssignedPrimaryId,
                mockReplicaCheckpointEntry.Object,
                mockReplicaAofBeginAddress,
                mockReplicaAofTailAddress,
                mockLogger.Object
            );

            // Act
            var result = await replicaSyncSession.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Checkpoint search completed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
