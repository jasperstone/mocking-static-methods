using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;

namespace Garnet.cluster.Tests
{
    public class CheckpointStoreTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly CheckpointStore _checkpointStore;

        public CheckpointStoreTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _checkpointStore = new CheckpointStore(null, _clusterProviderMock.Object, false, _loggerMock.Object);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsCheckpointEntry()
        {
            // Arrange
            var entry = new CheckpointEntry();

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsDeletingLogToken()
        {
            // Arrange
            var entry = new CheckpointEntry();
            var ckptManagerMock = new Mock<IReplicationLogCheckpointManager>();
            ckptManagerMock.Setup(x => x.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });
            _clusterProviderMock.Setup(x => x.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(ckptManagerMock.Object);

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsDeletingIndexToken()
        {
            // Arrange
            var entry = new CheckpointEntry();
            var ckptManagerMock = new Mock<IReplicationLogCheckpointManager>();
            ckptManagerMock.Setup(x => x.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });
            _clusterProviderMock.Setup(x => x.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(ckptManagerMock.Object);

            // Act
            _checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }
    }
}
