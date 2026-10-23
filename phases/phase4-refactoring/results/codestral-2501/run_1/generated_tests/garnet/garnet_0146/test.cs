using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class CheckpointStoreTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<ReplicationLogCheckpointManager> _ckptManagerMock;

        public CheckpointStoreTests()
        {
            _loggerMock = new Mock<ILogger>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _ckptManagerMock = new Mock<ReplicationLogCheckpointManager>();

            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(_ckptManagerMock.Object);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_WithNullEntry_FetchesLatestCheckpoint()
        {
            // Arrange
            var checkpointStore = new CheckpointStore(_storeWrapperMock.Object, _clusterProviderMock.Object, false, _loggerMock.Object);
            var latestCheckpoint = new CheckpointEntry();
            _ckptManagerMock.Setup(cm => cm.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid() });
            _ckptManagerMock.Setup(cm => cm.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid() });

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(null);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsCheckpointEntry()
        {
            // Arrange
            var checkpointStore = new CheckpointStore(_storeWrapperMock.Object, _clusterProviderMock.Object, false, _loggerMock.Object);
            var entry = new CheckpointEntry();
            _ckptManagerMock.Setup(cm => cm.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid() });
            _ckptManagerMock.Setup(cm => cm.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid() });

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_PurgesCheckpointsExceptSpecifiedEntry()
        {
            // Arrange
            var checkpointStore = new CheckpointStore(_storeWrapperMock.Object, _clusterProviderMock.Object, false, _loggerMock.Object);
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
            var logTokens = new[] { Guid.NewGuid(), entry.metadata.storeHlogToken };
            var indexTokens = new[] { Guid.NewGuid(), entry.metadata.storeIndexToken };
            _ckptManagerMock.Setup(cm => cm.GetLogCheckpointTokens()).Returns(logTokens);
            _ckptManagerMock.Setup(cm => cm.GetIndexCheckpointTokens()).Returns(indexTokens);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _ckptManagerMock.Verify(cm => cm.DeleteLogCheckpoint(It.IsAny<Guid>()), Times.Once);
            _ckptManagerMock.Verify(cm => cm.DeleteIndexCheckpoint(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_HandlesDisableObjectsOption()
        {
            // Arrange
            var checkpointStore = new CheckpointStore(_storeWrapperMock.Object, _clusterProviderMock.Object, false, _loggerMock.Object);
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
            var logTokens = new[] { Guid.NewGuid(), entry.metadata.storeHlogToken };
            var indexTokens = new[] { Guid.NewGuid(), entry.metadata.storeIndexToken };
            _ckptManagerMock.Setup(cm => cm.GetLogCheckpointTokens()).Returns(logTokens);
            _ckptManagerMock.Setup(cm => cm.GetIndexCheckpointTokens()).Returns(indexTokens);
            _clusterProviderMock.Setup(cp => cp.serverOptions.DisableObjects).Returns(true);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _ckptManagerMock.Verify(cm => cm.DeleteLogCheckpoint(It.IsAny<Guid>()), Times.Once);
            _ckptManagerMock.Verify(cm => cm.DeleteIndexCheckpoint(It.IsAny<Guid>()), Times.Once);
        }
    }
}
