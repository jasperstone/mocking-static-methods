using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        private Mock<ClusterProvider> _clusterProviderMock;
        private Mock<ILogger> _loggerMock;
        private Mock<StoreWrapper> _storeWrapperMock;
        private Mock<ReplicationLogCheckpointManager> _ckptManagerMock;
        private CheckpointStore _checkpointStore;
        private CheckpointEntry _entry;

        public CheckpointStoreTests()
        {
            _clusterProviderMock = new Mock<ClusterProvider>();
            _loggerMock = new Mock<ILogger>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _ckptManagerMock = new Mock<ReplicationLogCheckpointManager>();

            // Setup cluster provider to return mock checkpoint manager
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_ckptManagerMock.Object);

            // Setup store wrapper to return mock checkpoint manager
            _storeWrapperMock.Setup(sw => sw.StoreCheckpointManager).Returns(_ckptManagerMock.Object);
            _storeWrapperMock.Setup(sw => sw.ObjectStoreCheckpointManager).Returns(_ckptManagerMock.Object);
            _storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new ServerOptions { DisableObjects = false });

            _checkpointStore = new CheckpointStore(_storeWrapperMock.Object, _clusterProviderMock.Object, false, _loggerMock.Object);

            // Setup a sample checkpoint entry
            _entry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeHlogToken = Guid.NewGuid(),
                    storeIndexToken = Guid.NewGuid(),
                    objectStoreHlogToken = Guid.NewGuid(),
                    objectStoreIndexToken = Guid.NewGuid()
                }
            };
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_ShouldLogTraceAndDeleteTokens()
        {
            // Arrange
            var logTokens = new List<Guid> { Guid.NewGuid(), _entry.metadata.storeHlogToken };
            var indexTokens = new List<Guid> { Guid.NewGuid(), _entry.metadata.storeIndexToken };
            _ckptManagerMock.Setup(m => m.GetLogCheckpointTokens()).Returns(logTokens);
            _ckptManagerMock.Setup(m => m.GetIndexCheckpointTokens()).Returns(indexTokens);

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(_entry);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogCheckpointEntry(LogLevel.Trace, nameof(_checkpointStore.PurgeAllCheckpointsExceptEntry), _entry),
                Times.Once);
            _ckptManagerMock.Verify(m => m.DeleteLogCheckpoint(It.Is<Guid>(g => g != _entry.metadata.storeHlogToken)), Times.Exactly(logTokens.Count - 1));
            _ckptManagerMock.Verify(m => m.DeleteIndexCheckpoint(It.Is<Guid>(g => g != _entry.metadata.storeIndexToken)), Times.Exactly(indexTokens.Count - 1));
            _loggerMock.Verify(logger => logger.LogTrace(It.IsAny<string>(), It.IsAny<Guid>()), Times.Exactly(logTokens.Count + indexTokens.Count - 2));
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_ShouldNotLogIfEntryIsNull()
        {
            // Arrange
            _loggerMock.Setup(logger => logger.LogCheckpointEntry(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<CheckpointEntry>()));

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(null);

            // Assert
            _loggerMock.Verify(logger => logger.LogCheckpointEntry(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<CheckpointEntry>()), Times.Never);
        }
    }

    // Dummy classes to compile the test
    public class CheckpointEntry
    {
        public CheckpointMetadata metadata { get; set; }
        public CheckpointEntry next { get; set; }
    }

    public class CheckpointMetadata
    {
        public Guid storeHlogToken { get; set; }
        public Guid storeIndexToken { get; set; }
        public Guid objectStoreHlogToken { get; set; }
        public Guid objectStoreIndexToken { get; set; }
        public int storeVersion { get; set; } = 0;
        public int objectStoreVersion { get; set; } = 0;
        public string storeCheckpointCoveredAofAddress { get; set; } = "address";
        public Guid storePrimaryReplId { get; set; } = Guid.NewGuid();
    }

    public class ServerOptions
    {
        public bool DisableObjects { get; set; }
    }
}
