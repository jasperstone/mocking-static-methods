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

        private class FakeClusterProvider
        {
            public ServerOptions serverOptions;
            public FakeCheckpointManager mainManager;
            public FakeCheckpointManager objectManager;

            public FakeClusterProvider(ServerOptions options, FakeCheckpointManager mainManager, FakeCheckpointManager objectManager)
            {
                this.serverOptions = options;
                this.mainManager = mainManager;
                this.objectManager = objectManager;
            }

            public object GetReplicationLogCheckpointManager(StoreType storeType)
            {
                return storeType == StoreType.Main ? mainManager : objectManager;
            }
        }

        private CheckpointEntry CreateCheckpointEntryWithTokens(Guid storeHlogToken, Guid storeIndexToken, Guid objectStoreHlogToken, Guid objectStoreIndexToken)
        {
            var entry = new CheckpointEntry();
            entry.metadata.storeHlogToken = storeHlogToken;
            entry.metadata.storeIndexToken = storeIndexToken;
            entry.metadata.objectStoreHlogToken = objectStoreHlogToken;
            entry.metadata.objectStoreIndexToken = objectStoreIndexToken;
            return entry;
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForDeletingIndexToken()
        {
            // Arrange
            var logTokenToKeep = Guid.NewGuid();
            var indexTokenToKeep = Guid.NewGuid();

            var logTokensMain = new List<Guid> { logTokenToKeep, Guid.NewGuid() };
            var indexTokensMain = new List<Guid> { indexTokenToKeep, Guid.NewGuid() };

            var logTokensObject = new List<Guid> { Guid.NewGuid() };
            var indexTokensObject = new List<Guid> { Guid.NewGuid() };

            var mainManager = new FakeCheckpointManager(logTokensMain, indexTokensMain);
            var objectManager = new FakeCheckpointManager(logTokensObject, indexTokensObject);

            var serverOptions = new ServerOptions { DisableObjects = false };
            var clusterProvider = new FakeClusterProvider(serverOptions, mainManager, objectManager);

            var loggerMock = new Mock<ILogger>();

            var storeWrapper = new StoreWrapper(null, null, null, null, null, null, null, null, null);

            var checkpointStore = new CheckpointStore(storeWrapper, clusterProvider, safelyRemoveOutdated: false, loggerMock.Object);

            var entry = CreateCheckpointEntryWithTokens(logTokenToKeep, indexTokenToKeep, logTokensObject[0], indexTokensObject[0]);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            // Verify that LogTrace was called for deleting log tokens except the one to keep
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting log token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(logTokensMain.Count - 1));

            // Verify that LogTrace was called for deleting index tokens except the one to keep
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting index token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(indexTokensMain.Count - 1));
        }
    }
}
