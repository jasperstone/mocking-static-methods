using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_NullEntry_FetchesLatestFromDisk()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var checkpointStore = new CheckpointStore(mockStoreWrapper.Object, mockClusterProvider.Object, false, mockLogger.Object);

            var latestCheckpoint = new CheckpointEntry();
            mockClusterProvider.Setup(cp => cp.GetLatestCheckpointEntryFromDisk()).Returns(latestCheckpoint);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(null);

            // Assert
            mockClusterProvider.Verify(cp => cp.GetLatestCheckpointEntryFromDisk(), Times.Once);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsCheckpointEntry()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var checkpointStore = new CheckpointStore(mockStoreWrapper.Object, mockClusterProvider.Object, false, mockLogger.Object);

            var checkpointEntry = new CheckpointEntry();
            mockClusterProvider.Setup(cp => cp.GetLatestCheckpointEntryFromDisk()).Returns(checkpointEntry);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            mockLogger.Verify(logger => logger.LogCheckpointEntry(LogLevel.Trace, "PurgeAllCheckpointsExceptEntry", checkpointEntry), Times.Once);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_PurgesCheckpointsExceptSpecifiedEntry()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var checkpointStore = new CheckpointStore(mockStoreWrapper.Object, mockClusterProvider.Object, false, mockLogger.Object);

            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeHlogToken = Guid.NewGuid(),
                    storeIndexToken = Guid.NewGuid(),
                    objectStoreHlogToken = Guid.NewGuid(),
                    objectStoreIndexToken = Guid.NewGuid()
                }
            };

            var mockCkptManager = new Mock<IReplicationLogCheckpointManager>();
            mockCkptManager.Setup(cm => cm.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });
            mockCkptManager.Setup(cm => cm.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });

            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(mockCkptManager.Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(mockCkptManager.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            mockCkptManager.Verify(cm => cm.DeleteLogCheckpoint(It.IsAny<Guid>()), Times.Exactly(2));
            mockCkptManager.Verify(cm => cm.DeleteIndexCheckpoint(It.IsAny<Guid>()), Times.Exactly(2));
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_DisableObjects_PurgesOnlyMainCheckpoints()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var checkpointStore = new CheckpointStore(mockStoreWrapper.Object, mockClusterProvider.Object, false, mockLogger.Object);

            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeHlogToken = Guid.NewGuid(),
                    storeIndexToken = Guid.NewGuid()
                }
            };

            var mockCkptManager = new Mock<IReplicationLogCheckpointManager>();
            mockCkptManager.Setup(cm => cm.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });
            mockCkptManager.Setup(cm => cm.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });

            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(mockCkptManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions.DisableObjects).Returns(true);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            mockCkptManager.Verify(cm => cm.DeleteLogCheckpoint(It.IsAny<Guid>()), Times.Exactly(2));
            mockCkptManager.Verify(cm => cm.DeleteIndexCheckpoint(It.IsAny<Guid>()), Times.Exactly(2));
            mockClusterProvider.Verify(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object), Times.Never);
        }
    }
}
