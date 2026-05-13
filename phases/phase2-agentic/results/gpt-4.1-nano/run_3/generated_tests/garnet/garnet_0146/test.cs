using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
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

        public CheckpointStoreTests()
        {
            _clusterProviderMock = new Mock<ClusterProvider>();
            _loggerMock = new Mock<ILogger>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _ckptManagerMock = new Mock<ReplicationLogCheckpointManager>();

            // Setup clusterProvider to return mock checkpoint manager
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_ckptManagerMock.Object);

            // Setup storeWrapper to return mock checkpoint managers
            _storeWrapperMock.Setup(sw => sw.StoreCheckpointManager).Returns(new Mock<CheckpointManager>().Object);
            _storeWrapperMock.Setup(sw => sw.ObjectStoreCheckpointManager).Returns(new Mock<CheckpointManager>().Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { DisableObjects = false });
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(new Mock<ReplicationManager>().Object);

            _checkpointStore = new CheckpointStore(_storeWrapperMock.Object, _clusterProviderMock.Object, false, _loggerMock.Object);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_ShouldLogTrace_WhenCalled()
        {
            // Arrange
            var mockEntry = new CheckpointEntry
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
            _checkpointStore.PurgeAllCheckpointsExceptEntry(mockEntry);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogCheckpointEntry(
                    LogLevel.Trace,
                    nameof(_checkpointStore.PurgeAllCheckpointsExceptEntry),
                    It.IsAny<CheckpointEntry>()),
                Times.Once);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_ShouldCallDeleteLogAndIndex_WhenTokensDiffer()
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

            var mockCkptManager = new Mock<ReplicationLogCheckpointManager>();
            mockCkptManager.Setup(m => m.GetLogCheckpointTokens()).Returns(new List<Guid> { entry.metadata.storeHlogToken, Guid.NewGuid() });
            mockCkptManager.Setup(m => m.GetIndexCheckpointTokens()).Returns(new List<Guid> { entry.metadata.storeIndexToken, Guid.NewGuid() });

            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main))
                .Returns(mockCkptManager.Object);
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object))
                .Returns(mockCkptManager.Object);

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            mockCkptManager.Verify(m => m.DeleteLogCheckpoint(It.Is<Guid>(g => g != entry.metadata.storeHlogToken)), Times.Once);
            mockCkptManager.Verify(m => m.DeleteIndexCheckpoint(It.Is<Guid>(g => g != entry.metadata.storeIndexToken)), Times.Once);
        }

        [Fact]
        public void AddCheckpointEntry_ShouldLogTrace_WhenAddingEntry()
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

            // Act
            _checkpointStore.AddCheckpointEntry(entry, fullCheckpoint: true);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogCheckpointEntry(LogLevel.Trace, nameof(_checkpointStore.AddCheckpointEntry), entry),
                Times.Once);
        }
    }

    // Dummy classes to compile the test
    public class CheckpointEntry
    {
        public CheckpointMetadata metadata;
        public CheckpointEntry next;
    }

    public class CheckpointMetadata
    {
        public Guid storeHlogToken;
        public Guid storeIndexToken;
        public Guid objectStoreHlogToken;
        public Guid objectStoreIndexToken;
        public long storeVersion = 0;
        public long objectStoreVersion = 0;
        public string storeCheckpointCoveredAofAddress;
        public string storePrimaryReplId;
    }

    public enum StoreType
    {
        Main,
        Object
    }

    public class ServerOptions
    {
        public bool DisableObjects { get; set; }
    }

    public class CheckpointManager
    {
        public virtual IEnumerable<Guid> GetLogCheckpointTokens() => new List<Guid>();
        public virtual IEnumerable<Guid> GetIndexCheckpointTokens() => new List<Guid>();
        public virtual void DeleteLogCheckpoint(Guid token) { }
        public virtual void DeleteIndexCheckpoint(Guid token) { }
    }

    public class ClusterProvider
    {
        public StoreWrapper storeWrapper { get; set; }
        public ServerOptions serverOptions { get; set; }
        public IReplicationManager replicationManager { get; set; }

        public CheckpointManager GetReplicationLogCheckpointManager(StoreType storeType) => new CheckpointManager();
    }

    public class StoreWrapper
    {
        public CheckpointManager StoreCheckpointManager { get; set; }
        public CheckpointManager ObjectStoreCheckpointManager { get; set; }
        public ServerOptions serverOptions { get; set; }
    }

    public interface IReplicationManager
    {
        bool TryAcquireSettledMetadataForMainStore(CheckpointEntry entry, out object a, out object b);
        bool TryAcquireSettledMetadataForObjectStore(CheckpointEntry entry, out object a, out object b);
    }

    public class GarnetException : Exception
    {
        public GarnetException(string message) : base(message) { }
    }
}
