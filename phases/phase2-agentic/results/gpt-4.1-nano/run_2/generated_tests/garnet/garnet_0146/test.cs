using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        private class DummyCheckpointEntry : CheckpointEntry
        {
            public DummyCheckpointEntry(Guid storeHlogToken, Guid storeIndexToken, Guid objectStoreHlogToken, Guid objectStoreIndexToken)
            {
                metadata = new CheckpointMetadata
                {
                    storeHlogToken = storeHlogToken,
                    storeIndexToken = storeIndexToken,
                    objectStoreHlogToken = objectStoreHlogToken,
                    objectStoreIndexToken = objectStoreIndexToken
                };
            }
        }

        private class DummyCkptManager
        {
            public List<Guid> LogTokens { get; } = new List<Guid>();
            public List<Guid> IndexTokens { get; } = new List<Guid>();
            public void AddLogToken(Guid token) => LogTokens.Add(token);
            public void AddIndexToken(Guid token) => IndexTokens.Add(token);
            public IEnumerable<Guid> GetLogCheckpointTokens() => LogTokens;
            public IEnumerable<Guid> GetIndexCheckpointTokens() => IndexTokens;
            public void DeleteLogCheckpoint(Guid token) => LogTokens.Remove(token);
            public void DeleteIndexCheckpoint(Guid token) => IndexTokens.Remove(token);
        }

        private class DummyClusterProvider
        {
            public DummyCkptManager MainManager { get; } = new DummyCkptManager();
            public DummyCkptManager ObjectManager { get; } = new DummyCkptManager();

            public DummyClusterProvider()
            {
                GetReplicationLogCheckpointManager = (StoreType storeType) =>
                {
                    if (storeType == StoreType.Main)
                        return MainManager;
                    else
                        return ObjectManager;
                };
            }

            public Func<StoreType, DummyCkptManager> GetReplicationLogCheckpointManager { get; }
            public Mock<IReplicationManager> replicationManager = new Mock<IReplicationManager>();
            public Mock<IStoreWrapper> storeWrapper = new Mock<IStoreWrapper>();
            public Mock<IClusterOptions> serverOptions = new Mock<IClusterOptions>();
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_ShouldLogTraceAndDeleteTokens()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var storeWrapper = clusterProvider.storeWrapper.Object;
            var optionsMock = new Mock<IClusterOptions>();
            optionsMock.SetupGet(o => o.DisableObjects).Returns(false);
            clusterProvider.serverOptions = optionsMock;

            var checkpointStore = new CheckpointStore(storeWrapper, clusterProvider, false, loggerMock.Object);

            var entry = new DummyCheckpointEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            // Add some tokens to delete
            var logTokens = new List<Guid> { Guid.NewGuid(), entry.metadata.storeHlogToken, Guid.NewGuid() };
            var indexTokens = new List<Guid> { Guid.NewGuid(), entry.metadata.storeIndexToken, Guid.NewGuid() };
            clusterProvider.MainManager.LogTokens.AddRange(logTokens);
            clusterProvider.MainManager.IndexTokens.AddRange(indexTokens);
            clusterProvider.ObjectManager.LogTokens.AddRange(logTokens);
            clusterProvider.ObjectManager.IndexTokens.AddRange(indexTokens);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            // The tokens that are not equal to entry's tokens should be deleted
            Assert.DoesNotContain(entry.metadata.storeHlogToken, clusterProvider.MainManager.LogTokens);
            Assert.DoesNotContain(entry.metadata.storeHlogToken, clusterProvider.ObjectManager.LogTokens);
            Assert.DoesNotContain(entry.metadata.storeIndexToken, clusterProvider.MainManager.IndexTokens);
            Assert.DoesNotContain(entry.metadata.storeIndexToken, clusterProvider.ObjectManager.IndexTokens);

            // Verify that LogTrace was called for each token deletion
            loggerMock.Verify(
                logger => logger.LogTrace(It.Is<string>(s => s.Contains("Deleting log token")), It.IsAny<Guid>()),
                Times.Exactly(logTokens.Count - 1));
            loggerMock.Verify(
                logger => logger.LogTrace(It.Is<string>(s => s.Contains("Deleting index token")), It.IsAny<Guid>()),
                Times.Exactly(indexTokens.Count - 1));
        }
    }
}
