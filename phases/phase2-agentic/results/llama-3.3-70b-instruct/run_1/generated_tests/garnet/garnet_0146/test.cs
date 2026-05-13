using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using System;

public class CheckpointStoreTests
{
    [Fact]
    public void PurgeAllCheckpointsExceptEntry_LogTrace_Called()
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
    public void PurgeAllCheckpointsExceptEntry_LogCheckpointEntry_Called()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var checkpointStore = new CheckpointStore(null, null, false, loggerMock.Object);
        var entry = new CheckpointEntry();

        // Act
        checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

        // Assert
        loggerMock.Verify(l => l.LogCheckpointEntry(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<CheckpointEntry>()), Times.Once);
    }

    [Fact]
    public void AddCheckpointEntry_LogCheckpointEntry_Called()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var checkpointStore = new CheckpointStore(null, null, false, loggerMock.Object);
        var entry = new CheckpointEntry();

        // Act
        checkpointStore.AddCheckpointEntry(entry);

        // Assert
        loggerMock.Verify(l => l.LogCheckpointEntry(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<CheckpointEntry>()), Times.Once);
    }
}
