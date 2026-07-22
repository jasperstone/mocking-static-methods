using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CheckpointStore>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var checkpointManagerMock = new Mock<ReplicationCheckpointManager>();
            var objectCheckpointManagerMock = new Mock<ReplicationCheckpointManager>();

            // Setup clusterProvider to return mock checkpoint managers
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(checkpointManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object))
                .Returns(objectCheckpointManagerMock.Object);

            // Setup checkpoint manager to return tokens
            checkpointManagerMock.Setup(m => m.GetLogCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
            checkpointManagerMock.Setup(m => m.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
            checkpointManagerMock.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>()));
            checkpointManagerMock.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>()));

            objectCheckpointManagerMock.Setup(m => m.GetLogCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid() });
            objectCheckpointManagerMock.Setup(m => m.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid() });
            objectCheckpointManagerMock.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>()));
            objectCheckpointManagerMock.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>()));

            // Setup GetLatestCheckpointEntryFromDisk to return a dummy entry
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

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);
            // Override GetLatestCheckpointEntryFromDisk
            var checkpointStoreType = typeof(CheckpointStore);
            var method = checkpointStoreType.GetMethod("PurgeAllCheckpointsExceptEntry");
            // Call the method directly with the dummy entry
            checkpointStore.PurgeAllCheckpointsExceptEntry(dummyEntry);

            // Act
            // The method should log a trace message
            // No explicit assert needed, verify that LogTrace was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("PurgeAllCheckpointsExceptEntry")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
