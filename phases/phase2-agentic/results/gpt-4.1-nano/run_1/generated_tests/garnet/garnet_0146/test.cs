using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        private class DummyCheckpointEntry : CheckpointEntry
        {
            public override CheckpointEntry next { get; set; }
            public override CheckpointEntry next { get; set; }
            public DummyCheckpointEntry(Guid storeHlogToken, Guid storeIndexToken, Guid objectStoreHlogToken, Guid objectStoreIndexToken)
            {
                this.metadata = new CheckpointMetadata
                {
                    storeHlogToken = storeHlogToken,
                    storeIndexToken = storeIndexToken,
                    objectStoreHlogToken = objectStoreHlogToken,
                    objectStoreIndexToken = objectStoreIndexToken,
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storeCheckpointCoveredAofAddress = 0,
                    storePrimaryReplId = Guid.NewGuid()
                };
            }
        }

        private class DummyClusterProvider : ClusterProvider
        {
            public override IReplicationLogCheckpointManager GetReplicationLogCheckpointManager(StoreType storeType)
            {
                var mock = new Mock<IReplicationLogCheckpointManager>();
                var tokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
                mock.Setup(m => m.GetLogCheckpointTokens()).Returns(tokens);
                mock.Setup(m => m.GetIndexCheckpointTokens()).Returns(tokens);
                mock.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>())).Verifiable();
                mock.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>())).Verifiable();
                return mock.Object;
            }

            public override bool TryAcquireSettledMetadataForMainStore(CheckpointEntry entry, out object a, out object b)
            {
                a = b = null;
                return true;
            }

            public override bool TryAcquireSettledMetadataForObjectStore(CheckpointEntry entry, out object a, out object b)
            {
                a = b = null;
                return true;
            }

            public override StoreWrapper storeWrapper { get; }
            public override ServerOptions serverOptions { get; }
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_ShouldLogTraceAndDeleteTokens()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var mockCkptManager = new Mock<IReplicationLogCheckpointManager>();
            var logTokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var indexTokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            mockCkptManager.Setup(m => m.GetLogCheckpointTokens()).Returns(logTokens);
            mockCkptManager.Setup(m => m.GetIndexCheckpointTokens()).Returns(indexTokens);
            mockCkptManager.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>())).Verifiable();
            mockCkptManager.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>())).Verifiable();

            clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockCkptManager.Object);

            var storeWrapper = new StoreWrapper(); // dummy, not used directly
            var options = new ServerOptions { DisableObjects = false };
            var clusterProvider = new DummyClusterProvider
            {
                storeWrapper = storeWrapper,
                serverOptions = options
            };

            var checkpointStore = new CheckpointStore(storeWrapper, clusterProvider, false, loggerMock.Object);

            var entry = new DummyCheckpointEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            loggerMock.Verify(l => l.LogCheckpointEntry(LogLevel.Trace, nameof(CheckpointStore.PurgeAllCheckpointsExceptEntry), It.IsAny<CheckpointEntry>()), Times.Once);
            mockCkptManager.Verify(m => m.GetLogCheckpointTokens(), Times.Once);
            mockCkptManager.Verify(m => m.GetIndexCheckpointTokens(), Times.Once);
            foreach (var token in logTokens)
            {
                mockCkptManager.Verify(m => m.DeleteLogCheckpoint(token), Times.Once);
            }
            foreach (var token in indexTokens)
            {
                mockCkptManager.Verify(m => m.DeleteIndexCheckpoint(token), Times.Once);
            }
        }
    }
}
