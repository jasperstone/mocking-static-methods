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
        private class DummyCheckpointManager : IReplicationLogCheckpointManager
        {
            private readonly List<Guid> logTokens;
            private readonly List<Guid> indexTokens;
            public readonly List<Guid> DeletedLogTokens = new();
            public readonly List<Guid> DeletedIndexTokens = new();

            public DummyCheckpointManager(List<Guid> logTokens, List<Guid> indexTokens)
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

        private class DummyClusterProvider : ClusterProvider
        {
            public DummyCheckpointManager MainManager;
            public DummyCheckpointManager ObjectManager;
            public ServerOptions serverOptions;

            public DummyClusterProvider()
            {
                serverOptions = new ServerOptions();
                base.serverOptions = serverOptions;
            }

            public override IReplicationLogCheckpointManager GetReplicationLogCheckpointManager(StoreType storeType)
            {
                return storeType == StoreType.Main ? MainManager : ObjectManager;
            }
        }

        private CheckpointEntry CreateCheckpointEntryWithTokens(Guid logToken, Guid indexToken, Guid objLogToken, Guid objIndexToken)
        {
            var entry = new CheckpointEntry();
            entry.metadata.storeHlogToken = logToken;
            entry.metadata.storeIndexToken = indexToken;
            entry.metadata.objectStoreHlogToken = objLogToken;
            entry.metadata.objectStoreIndexToken = objIndexToken;
            return entry;
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForDeletingIndexTokens()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();

            var logTokenToKeep = Guid.NewGuid();
            var indexTokenToKeep = Guid.NewGuid();
            var objLogTokenToKeep = Guid.NewGuid();
            var objIndexTokenToKeep = Guid.NewGuid();

            var logTokens = new List<Guid> { logTokenToKeep, Guid.NewGuid() };
            var indexTokens = new List<Guid> { indexTokenToKeep, Guid.NewGuid() };
            var objLogTokens = new List<Guid> { objLogTokenToKeep, Guid.NewGuid() };
            var objIndexTokens = new List<Guid> { objIndexTokenToKeep, Guid.NewGuid() };

            var mainManager = new DummyCheckpointManager(logTokens, indexTokens);
            var objectManager = new DummyCheckpointManager(objLogTokens, objIndexTokens);

            clusterProvider.MainManager = mainManager;
            clusterProvider.ObjectManager = objectManager;
            clusterProvider.serverOptions.DisableObjects = false;

            var entry = CreateCheckpointEntryWithTokens(logTokenToKeep, indexTokenToKeep, objLogTokenToKeep, objIndexTokenToKeep);

            var storeWrapperMock = new Mock<StoreWrapper>();
            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProvider, safelyRemoveOutdated: false, loggerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            // Check that LogTrace was called for deleting log tokens except the one to keep
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting log token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // One for main store log token, one for object store log token

            // Check that LogTrace was called for deleting index tokens except the one to keep
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting index token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // One for main store index token, one for object store index token

            // Check that the correct tokens were deleted from mainManager
            Assert.Contains(mainManager.DeletedLogTokens, t => !t.Equals(logTokenToKeep));
            Assert.Contains(mainManager.DeletedIndexTokens, t => !t.Equals(indexTokenToKeep));

            // Check that the correct tokens were deleted from objectManager
            Assert.Contains(objectManager.DeletedLogTokens, t => !t.Equals(objLogTokenToKeep));
            Assert.Contains(objectManager.DeletedIndexTokens, t => !t.Equals(objIndexTokenToKeep));
        }
    }
}
