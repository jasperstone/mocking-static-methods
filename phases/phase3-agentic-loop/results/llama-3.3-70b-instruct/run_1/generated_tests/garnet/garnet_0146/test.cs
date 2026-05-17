using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogTraceCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var checkpointStore = new CheckpointStore(null, null, false, loggerMock.Object);
            var checkpointEntry = new CheckpointEntry();

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void AddCheckpointEntry_LogTraceCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var checkpointStore = new CheckpointStore(null, null, false, loggerMock.Object);
            var checkpointEntry = new CheckpointEntry();

            // Act
            checkpointStore.AddCheckpointEntry(checkpointEntry);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}
