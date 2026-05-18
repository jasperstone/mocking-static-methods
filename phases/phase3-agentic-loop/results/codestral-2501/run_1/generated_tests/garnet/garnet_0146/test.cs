using System;
using System.Collections.Generic;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var checkpointManagerMock = new Mock<ReplicationLogCheckpointManager>();

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

            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(checkpointManagerMock.Object);

            checkpointManagerMock.Setup(cm => cm.GetLogCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            checkpointManagerMock.Setup(cm => cm.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Trace),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(4));
        }
    }
}
