using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogTraceCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var checkpointStore = new CheckpointStore(null, null, false, loggerMock.Object);
            var entry = new CheckpointEntry();

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void AddCheckpointEntry_LogTraceCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var checkpointStore = new CheckpointStore(null, null, false, loggerMock.Object);
            var entry = new CheckpointEntry();

            // Act
            checkpointStore.AddCheckpointEntry(entry);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}
