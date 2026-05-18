using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationLogCheckpointManagerMock = new Mock<ReplicationLogCheckpointManager>();
            var checkpointEntry = new CheckpointEntry(new CheckpointMetadata(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                0,
                string.Empty,
                string.Empty,
                0,
                0));

            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(replicationLogCheckpointManagerMock.Object);

            replicationLogCheckpointManagerMock.Setup(rlcp => rlcp.GetLogCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            replicationLogCheckpointManagerMock.Setup(rlcp => rlcp.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, true, loggerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_DeletesLogCheckpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationLogCheckpointManagerMock = new Mock<ReplicationLogCheckpointManager>();
            var checkpointEntry = new CheckpointEntry(new CheckpointMetadata(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                0,
                string.Empty,
                string.Empty,
                0,
                0));

            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(replicationLogCheckpointManagerMock.Object);

            replicationLogCheckpointManagerMock.Setup(rlcp => rlcp.GetLogCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            replicationLogCheckpointManagerMock.Setup(rlcp => rlcp.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, true, loggerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            replicationLogCheckpointManagerMock.Verify(rlcp => rlcp.DeleteLogCheckpoint(It.IsAny<Guid>()), Times.AtLeastOnce);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_DeletesIndexCheckpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationLogCheckpointManagerMock = new Mock<ReplicationLogCheckpointManager>();
            var checkpointEntry = new CheckpointEntry(new CheckpointMetadata(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                0,
                string.Empty,
                string.Empty,
                0,
                0));

            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(replicationLogCheckpointManagerMock.Object);

            replicationLogCheckpointManagerMock.Setup(rlcp => rlcp.GetLogCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            replicationLogCheckpointManagerMock.Setup(rlcp => rlcp.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, true, loggerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            replicationLogCheckpointManagerMock.Verify(rlcp => rlcp.DeleteIndexCheckpoint(It.IsAny<Guid>()), Times.AtLeastOnce);
        }
    }
}
