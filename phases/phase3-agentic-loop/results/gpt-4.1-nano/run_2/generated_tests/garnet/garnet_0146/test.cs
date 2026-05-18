using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        private class DummyCheckpointEntry : CheckpointEntry
        {
            public override string ToString() => $"DummyEntry_{Guid.NewGuid()}";
        }

        private class DummyClusterProvider : ClusterProvider
        {
            public override IReplicationLogCheckpointManager GetReplicationLogCheckpointManager(StoreType storeType)
            {
                var mockManager = new Mock<IReplicationLogCheckpointManager>();
                var tokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
                mockManager.Setup(m => m.GetLogCheckpointTokens()).Returns(tokens);
                mockManager.Setup(m => m.GetIndexCheckpointTokens()).Returns(tokens);
                mockManager.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>())).Verifiable();
                mockManager.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>())).Verifiable();
                return mockManager.Object;
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

            public override StoreWrapper storeWrapper => new StoreWrapper();
        }

        private class DummyStoreWrapper : StoreWrapper
        {
            public override IStoreCheckpointManager StoreCheckpointManager => new DummyCheckpointManager();
            public override IObjectStoreCheckpointManager ObjectStoreCheckpointManager => new DummyObjectCheckpointManager();
            public override ServerOptions serverOptions => new ServerOptions { DisableObjects = false };
        }

        private class DummyCheckpointManager : IStoreCheckpointManager
        {
            public object RecoveredSafeAofAddress { get; set; }
            public object RecoveredHistoryId { get; set; }
            public List<Guid> GetLogCheckpointTokens() => new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            public List<Guid> GetIndexCheckpointTokens() => new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            public void DeleteLogCheckpoint(Guid token) { }
            public void DeleteIndexCheckpoint(Guid token) { }
        }

        private class DummyObjectCheckpointManager : IObjectStoreCheckpointManager
        {
            public object RecoveredSafeAofAddress { get; set; }
            public object RecoveredHistoryId { get; set; }
        }

        private class DummyServerOptions : ServerOptions
        {
            public override bool DisableObjects { get; set; } = false;
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_ShouldLogTraceAndDeleteTokens()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new DummyStoreWrapper();
            var clusterProvider = new DummyClusterProvider();
            var store = new CheckpointStore(storeWrapper, clusterProvider, true, loggerMock.Object);

            var dummyEntry = new CheckpointEntry
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
            store.PurgeAllCheckpointsExceptEntry(dummyEntry);

            // Assert
            loggerMock.Verify(
                l => l.LogCheckpointEntry(LogLevel.Trace, nameof(CheckpointStore.PurgeAllCheckpointsExceptEntry), dummyEntry),
                Times.Once);
        }
    }
}
