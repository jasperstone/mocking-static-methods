using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;

namespace Garnet.cluster.Tests
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
            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);

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

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(3)); // 1 for PurgeAllCheckpointsExceptEntry, 1 for each token deletion
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_DeletesTokens()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);

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

            var ckptManagerMock = new Mock<IReplicationLogCheckpointManager>();
            ckptManagerMock.Setup(x => x.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });
            ckptManagerMock.Setup(x => x.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });

            clusterProviderMock.Setup(x => x.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(ckptManagerMock.Object);
            clusterProviderMock.Setup(x => x.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(ckptManagerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            ckptManagerMock.Verify(x => x.DeleteLogCheckpoint(It.IsAny<Guid>()), Times.Exactly(2));
            ckptManagerMock.Verify(x => x.DeleteIndexCheckpoint(It.IsAny<Guid>()), Times.Exactly(2));
        }
    }
}
