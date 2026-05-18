using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForEachToken()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationLogCheckpointManagerMock = new Mock<ReplicationLogCheckpointManager>();

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, true, loggerMock.Object);

            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeHlogToken = Guid.NewGuid(),
                    storeIndexToken = Guid.NewGuid(),
                    objectStoreHlogToken = Guid.NewGuid(),
                    objectStoreIndexToken = Guid.NewGuid()
                }
            };

            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(replicationLogCheckpointManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(replicationLogCheckpointManagerMock.Object);

            replicationLogCheckpointManagerMock.Setup(rlcm => rlcm.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });
            replicationLogCheckpointManagerMock.Setup(rlcm => rlcm.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(4));
        }
    }
}
