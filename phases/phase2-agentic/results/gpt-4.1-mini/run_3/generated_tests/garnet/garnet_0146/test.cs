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
            public ServerOptions serverOptions;
            public DummyCheckpointManager mainManager;
            public DummyCheckpointManager objectManager;

            public DummyClusterProvider(ServerOptions options, DummyCheckpointManager mainManager, DummyCheckpointManager objectManager)
            {
                this.serverOptions = options;
                this.mainManager = mainManager;
                this.objectManager = objectManager;
            }

            public override ServerOptions serverOptions => this.serverOptions;

            public override IReplicationLogCheckpointManager GetReplicationLogCheckpointManager(StoreType storeType)
            {
                return storeType == StoreType.Main ? mainManager : objectManager;
            }
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForDeletedTokens()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mainLogTokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var mainIndexTokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var objectLogTokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var objectIndexTokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var mainManager = new DummyCheckpointManager(mainLogTokens, mainIndexTokens);
            var objectManager = new DummyCheckpointManager(objectLogTokens, objectIndexTokens);

            var serverOptions = new ServerOptions { DisableObjects = false };
            var clusterProvider = new DummyClusterProvider(serverOptions, mainManager, objectManager);

            var storeWrapper = new StoreWrapper(); // We don't use it in this test, so can be default
            var checkpointStore = new CheckpointStore(storeWrapper, clusterProvider, safelyRemoveOutdated: false, loggerMock.Object);

            // Create a checkpoint entry with tokens that match one token in each list, so others should be deleted
            var entry = new CheckpointEntry();
            entry.metadata.storeHlogToken = mainLogTokens[0];
            entry.metadata.storeIndexToken = mainIndexTokens[0];
            entry.metadata.objectStoreHlogToken = objectLogTokens[0];
            entry.metadata.objectStoreIndexToken = objectIndexTokens[0];

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            // Verify logger.LogCheckpointEntry called once at start with Trace level and correct method name
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(nameof(CheckpointStore.PurgeAllCheckpointsExceptEntry))),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify logger.LogTrace called for each deleted log token (all except the matching one)
            foreach (var token in mainLogTokens)
            {
                if (!token.Equals(entry.metadata.storeHlogToken))
                {
                    loggerMock.Verify(l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting log token") && v.ToString().Contains(token.ToString())),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                    Assert.Contains(token, mainManager.DeletedLogTokens);
                }
                else
                {
                    Assert.DoesNotContain(token, mainManager.DeletedLogTokens);
                }
            }

            // Verify logger.LogTrace called for each deleted index token (all except the matching one)
            foreach (var token in mainIndexTokens)
            {
                if (!token.Equals(entry.metadata.storeIndexToken))
                {
                    loggerMock.Verify(l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting index token") && v.ToString().Contains(token.ToString())),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                    Assert.Contains(token, mainManager.DeletedIndexTokens);
                }
                else
                {
                    Assert.DoesNotContain(token, mainManager.DeletedIndexTokens);
                }
            }

            // Verify object store tokens similarly
            foreach (var token in objectLogTokens)
            {
                if (!token.Equals(entry.metadata.objectStoreHlogToken))
                {
                    loggerMock.Verify(l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting log token") && v.ToString().Contains(token.ToString())),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                    Assert.Contains(token, objectManager.DeletedLogTokens);
                }
                else
                {
                    Assert.DoesNotContain(token, objectManager.DeletedLogTokens);
                }
            }

            foreach (var token in objectIndexTokens)
            {
                if (!token.Equals(entry.metadata.objectStoreIndexToken))
                {
                    loggerMock.Verify(l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting index token") && v.ToString().Contains(token.ToString())),
                        It.IsAny<Exception>(),
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
    }
}
