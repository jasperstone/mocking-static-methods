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

            public DummyClusterProvider(ServerOptions options) : base(null, null, null)
            {
                serverOptions = options;
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
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForDeletedTokens()
        {
            // Arrange
            var logTokenToKeep = Guid.NewGuid();
            var indexTokenToKeep = Guid.NewGuid();
            var objLogTokenToKeep = Guid.NewGuid();
            var objIndexTokenToKeep = Guid.NewGuid();

            var logTokens = new List<Guid> { logTokenToKeep, Guid.NewGuid(), Guid.NewGuid() };
            var indexTokens = new List<Guid> { indexTokenToKeep, Guid.NewGuid() };
            var objLogTokens = new List<Guid> { objLogTokenToKeep, Guid.NewGuid() };
            var objIndexTokens = new List<Guid> { objIndexTokenToKeep, Guid.NewGuid() };

            var mainManager = new DummyCheckpointManager(logTokens, indexTokens);
            var objectManager = new DummyCheckpointManager(objLogTokens, objIndexTokens);

            var serverOptions = new ServerOptions { DisableObjects = false };
            var clusterProvider = new DummyClusterProvider(serverOptions)
            {
                MainManager = mainManager,
                ObjectManager = objectManager
            };

            var loggerMock = new Mock<ILogger>();

            var storeWrapperMock = new Mock<StoreWrapper>();

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProvider, safelyRemoveOutdated: false, loggerMock.Object);

            var entry = CreateCheckpointEntryWithTokens(logTokenToKeep, indexTokenToKeep, objLogTokenToKeep, objIndexTokenToKeep);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            // Verify logger.LogTrace called for each deleted log token (except the one to keep)
            foreach (var token in logTokens)
            {
                if (!token.Equals(logTokenToKeep))
                {
                    loggerMock.Verify(
                        x => x.Log(
                            LogLevel.Trace,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting log token {token}")),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                    Assert.Contains(token, mainManager.DeletedLogTokens);
                }
                else
                {
                    Assert.DoesNotContain(token, mainManager.DeletedLogTokens);
                }
            }

            // Verify logger.LogTrace called for each deleted index token (except the one to keep)
            foreach (var token in indexTokens)
            {
                if (!token.Equals(indexTokenToKeep))
                {
                    loggerMock.Verify(
                        x => x.Log(
                            LogLevel.Trace,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting index token {token}")),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                    Assert.Contains(token, mainManager.DeletedIndexTokens);
                }
                else
                {
                    Assert.DoesNotContain(token, mainManager.DeletedIndexTokens);
                }
            }

            // Verify logger.LogTrace called for each deleted object store log token (except the one to keep)
            foreach (var token in objLogTokens)
            {
                if (!token.Equals(objLogTokenToKeep))
                {
                    loggerMock.Verify(
                        x => x.Log(
                            LogLevel.Trace,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting log token {token}")),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                    Assert.Contains(token, objectManager.DeletedLogTokens);
                }
                else
                {
                    Assert.DoesNotContain(token, objectManager.DeletedLogTokens);
                }
            }

            // Verify logger.LogTrace called for each deleted object store index token (except the one to keep)
            foreach (var token in objIndexTokens)
            {
                if (!token.Equals(objIndexTokenToKeep))
                {
                    loggerMock.Verify(
                        x => x.Log(
                            LogLevel.Trace,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting index token {token}")),
                            null,
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                    Assert.Contains(token, objectManager.DeletedIndexTokens);
                }
                else
                {
                    Assert.DoesNotContain(token, objectManager.DeletedIndexTokens);
                }
            }
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_DisableObjects_DoesNotPurgeObjectStore()
        {
            // Arrange
            var logTokenToKeep = Guid.NewGuid();
            var indexTokenToKeep = Guid.NewGuid();

            var logTokens = new List<Guid> { logTokenToKeep, Guid.NewGuid() };
            var indexTokens = new List<Guid> { indexTokenToKeep, Guid.NewGuid() };

            var mainManager = new DummyCheckpointManager(logTokens, indexTokens);

            var serverOptions = new ServerOptions { DisableObjects = true };
            var clusterProvider = new DummyClusterProvider(serverOptions)
            {
                MainManager = mainManager,
                ObjectManager = null // Should not be called
            };

            var loggerMock = new Mock<ILogger>();

            var storeWrapperMock = new Mock<StoreWrapper>();

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProvider, safelyRemoveOutdated: false, loggerMock.Object);

            var entry = CreateCheckpointEntryWithTokens(logTokenToKeep, indexTokenToKeep, Guid.Empty, Guid.Empty);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            // Only mainManager tokens are deleted, no object store calls
            Assert.All(mainManager.DeletedLogTokens, token => Assert.NotEqual(logTokenToKeep, token));
            Assert.All(mainManager.DeletedIndexTokens, token => Assert.NotEqual(indexTokenToKeep, token));
        }
    }
}
