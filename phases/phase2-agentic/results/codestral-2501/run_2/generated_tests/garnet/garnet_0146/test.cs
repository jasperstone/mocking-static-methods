using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;

namespace Garnet.cluster.Tests
{
    public class CheckpointStoreTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly CheckpointStore _checkpointStore;

        public CheckpointStoreTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _checkpointStore = new CheckpointStore(null, _clusterProviderMock.Object, false, _loggerMock.Object);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTrace()
        {
            // Arrange
            var entry = new CheckpointEntry();

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptTokens_DeletesCorrectTokens()
        {
            // Arrange
            var entry = new CheckpointEntry
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
            ckptManagerMock.Setup(x => x.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid(), entry.metadata.storeHlogToken });
            ckptManagerMock.Setup(x => x.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid(), entry.metadata.storeIndexToken });

            _clusterProviderMock.Setup(x => x.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(ckptManagerMock.Object);
            _clusterProviderMock.Setup(x => x.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(ckptManagerMock.Object);

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            ckptManagerMock.Verify(x => x.DeleteLogCheckpoint(It.IsAny<Guid>()), Times.Once);
            ckptManagerMock.Verify(x => x.DeleteIndexCheckpoint(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void AddCheckpointEntry_AddsNewEntry()
        {
            // Arrange
            var entry = new CheckpointEntry();

            // Act
            _checkpointStore.AddCheckpointEntry(entry);

            // Assert
            Assert.NotNull(_checkpointStore.head);
            Assert.NotNull(_checkpointStore.tail);
            Assert.Same(entry, _checkpointStore.tail);
        }
    }
}
