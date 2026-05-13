using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForDeletingIndexToken()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, safelyRemoveOutdated: true, logger: mockLogger.Object);

            var entry = new CheckpointEntry
            {
                metadata = new CheckpointEntryMetadata
                {
                    storeIndexToken = Guid.NewGuid(),
                    objectStoreIndexToken = Guid.NewGuid()
                }
            };

            var ckptManagerMock = new Mock<ReplicationLogCheckpointManager>();
            ckptManagerMock.Setup(m => m.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid(), entry.metadata.storeIndexToken });

            clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(ckptManagerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            mockLogger.Verify(
                l => l.LogTrace(
                    It.Is<string>(s => s == "Deleting index token {toDeleteIndexToken}"),
                    It.Is<Guid>(token => token != entry.metadata.storeIndexToken)),
                Times.Once);
        }
    }
}
