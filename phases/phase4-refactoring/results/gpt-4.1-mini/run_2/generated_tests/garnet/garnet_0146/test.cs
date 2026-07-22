using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class CheckpointStoreTests
    {
        private class FakeCheckpointManager
        {
            private readonly List<Guid> logTokens;
            private readonly List<Guid> indexTokens;
            public readonly List<Guid> DeletedLogTokens = new();
            public readonly List<Guid> DeletedIndexTokens = new();

            public FakeCheckpointManager(List<Guid> logTokens, List<Guid> indexTokens)
            {
                this.logTokens = logTokens;
                this.indexTokens = indexTokens;
            }

            public IEnumerable<Guid> GetLogCheckpointTokens() => logTokens;
            public IEnumerable<Guid> GetIndexCheckpointTokens() => indexTokens;

            public void DeleteLogCheckpoint(Guid token)
            {
                DeletedLogTokens.Add(token);
            }

            public void DeleteIndexCheckpoint(Guid token)
            {
                DeletedIndexTokens.Add(token);
            }
        }

        private class FakeClusterProvider : ClusterProvider
        {
            public ServerOptions serverOptions = new ServerOptions();
            public FakeCheckpointManager MainManager { get; }
            public FakeCheckpointManager ObjectManager { get; }

            public FakeClusterProvider(FakeCheckpointManager mainManager, FakeCheckpointManager objectManager, bool disableObjects)
            {
                MainManager = mainManager;
                ObjectManager = objectManager;
                serverOptions.DisableObjects = disableObjects;
            }

            public override IReplicationLogCheckpointManager GetReplicationLogCheckpointManager(StoreType storeType)
            {
                return storeType == StoreType.Main ? MainManager : ObjectManager;
            }
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForDeletedTokens()
        {
            // Arrange
            var logTokenToKeep = Guid.NewGuid();
            var indexTokenToKeep = Guid.NewGuid();

            var logTokens = new List<Guid> { logTokenToKeep, Guid.NewGuid() };
            var indexTokens = new List<Guid> { indexTokenToKeep, Guid.NewGuid() };

            var mainManager = new FakeCheckpointManager(logTokens, indexTokens);
            var objectManager = new FakeCheckpointManager(new List<Guid>(logTokens), new List<Guid>(indexTokens));

            var clusterProvider = new FakeClusterProvider(mainManager, objectManager, disableObjects: false);

            var loggerMock = new Mock<ILogger>();

            var storeWrapperMock = new Mock<StoreWrapper>();

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProvider, safelyRemoveOutdated: false, loggerMock.Object);

            var entry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeHlogToken = logTokenToKeep,
                    storeIndexToken = indexTokenToKeep,
                    objectStoreHlogToken = logTokenToKeep,
                    objectStoreIndexToken = indexTokenToKeep
                }
            };

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            // Verify logger.LogTrace called for each deleted log token except the one to keep
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting log token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // One for main log token, one for object log token

            // Verify logger.LogTrace called for each deleted index token except the one to keep
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting index token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // One for main index token, one for object index token
        }
    }
}
