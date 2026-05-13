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
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var checkpointManagerMock = new Mock<ReplicationLogCheckpointManager>();

            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(checkpointManagerMock.Object);

            checkpointManagerMock.Setup(cm => cm.GetIndexCheckpointTokens())
                .Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });

            var entry = new CheckpointEntry
            {
                metadata = new CheckpointEntryMetadata
                {
                    objectStoreIndexToken = Guid.NewGuid()
                }
            };

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, safelyRemoveOutdated: false, loggerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            loggerMock.Verify(
                logger => logger.LogTrace(
                    It.Is<string>(s => s.Contains("Deleting index token")),
                    It.IsAny<Guid>()
                ),
                Times.AtLeastOnce
            );
        }
    }
}
