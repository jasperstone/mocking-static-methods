using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class CheckpointStoreTests
    {
        private readonly Mock<StoreWrapper> _mockStoreWrapper;
        private readonly Mock<ClusterProvider> _mockClusterProvider;
        private readonly Mock<ILogger> _mockLogger;
        private readonly CheckpointStore _checkpointStore;

        public CheckpointStoreTests()
        {
            _mockStoreWrapper = new Mock<StoreWrapper>();
            _mockClusterProvider = new Mock<ClusterProvider>();
            _mockLogger = new Mock<ILogger>();

            _mockStoreWrapper.Setup(s => s.serverOptions).Returns(new ServerOptions { DisableObjects = true });

            _checkpointStore = new CheckpointStore(
                _mockStoreWrapper.Object,
                _mockClusterProvider.Object,
                safelyRemoveOutdated: false,
                _mockLogger.Object);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsIndexTokenDeletion_WhenIndexTokensDiffer()
        {
            // Arrange
            var entry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeIndexToken = Guid.NewGuid(),
                    storeHlogToken = Guid.NewGuid()
                }
            };

            var mockCkptManagerMain = new Mock<IRecoveryCheckpointManager>();
            var indexTokensMain = new List<Guid> { Guid.NewGuid(), entry.metadata.storeIndexToken };
            mockCkptManagerMain.Setup(m => m.GetIndexCheckpointTokens()).Returns(indexTokensMain);
            mockCkptManagerMain.Setup(m => m.GetLogCheckpointTokens()).Returns(new List<Guid>());
            _mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(mockCkptManagerMain.Object);

            var mockCkptManagerObject = new Mock<IRecoveryCheckpointManager>();
            mockCkptManagerObject.Setup(m => m.GetIndexCheckpointTokens()).Returns(new List<Guid>());
            mockCkptManagerObject.Setup(m => m.GetLogCheckpointTokens()).Returns(new List<Guid>());
            _mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(mockCkptManagerObject.Object);

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert - specifically verify the index token LogTrace call on line 111
            _mockLogger.Verify(
                l => l.LogTrace(
                    "Deleting index token {toDeleteIndexToken}",
                    It.Is<Guid>(g => g != entry.metadata.storeIndexToken),
                    Times.Once()));
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsLogTokenDeletion_WhenLogTokensDiffer()
        {
            // Arrange
            var entry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeHlogToken = Guid.NewGuid(),
                    storeIndexToken = Guid.NewGuid()
                }
            };

            var mockCkptManagerMain = new Mock<IRecoveryCheckpointManager>();
            var logTokensMain = new List<Guid> { Guid.NewGuid(), entry.metadata.storeHlogToken };
            mockCkptManagerMain.Setup(m => m.GetLogCheckpointTokens()).Returns(logTokensMain);
            mockCkptManagerMain.Setup(m => m.GetIndexCheckpointTokens()).Returns(new List<Guid>());
            _mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(mockCkptManagerMain.Object);

            var mockCkptManagerObject = new Mock<IRecoveryCheckpointManager>();
            mockCkptManagerObject.Setup(m => m.GetIndexCheckpointTokens()).Returns(new List<Guid>());
            mockCkptManagerObject.Setup(m => m.GetLogCheckpointTokens()).Returns(new List<Guid>());
            _mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(mockCkptManagerObject.Object);

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _mockLogger.Verify(
                l => l.LogTrace(
                    "Deleting log token {toDeletelogToken}",
                    It.Is<Guid>(g => g != entry.metadata.storeHlogToken),
                    Times.Once()));
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_DoesNotLogIndexDeletion_WhenNoIndexTokensToDelete()
        {
            // Arrange
            var entry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeIndexToken = Guid.NewGuid(),
                    storeHlogToken = Guid.NewGuid()
                }
            };

            var mockCkptManagerMain = new Mock<IRecoveryCheckpointManager>();
            mockCkptManagerMain.Setup(m => m.GetIndexCheckpointTokens()).Returns(new List<Guid> { entry.metadata.storeIndexToken });
            mockCkptManagerMain.Setup(m => m.GetLogCheckpointTokens()).Returns(new List<Guid>());
            _mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(mockCkptManagerMain.Object);

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert - no LogTrace calls for deletions
            _mockLogger.Verify(
                l => l.LogTrace(
                    It.Is<string>(s => s.Contains("Deleting")),
                    It.IsAny<object[]>(),
                    Times.Never()));
        }
    }
}
