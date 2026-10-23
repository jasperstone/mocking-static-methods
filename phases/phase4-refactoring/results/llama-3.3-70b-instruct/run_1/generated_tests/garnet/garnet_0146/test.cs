using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

public class CheckpointStoreTests
{
    [Fact]
    public void PurgeAllCheckpointsExceptEntry_LogsTrace()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);
        var checkpointEntry = new CheckpointEntry();

        // Act
        checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

        // Assert
        loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public void PurgeAllCheckpointsExceptEntry_DeletesLogCheckpoints()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);
        var checkpointEntry = new CheckpointEntry();
        var logCheckpointTokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

        // Assert
        // TODO: Verify that log checkpoints are deleted
    }

    [Fact]
    public void PurgeAllCheckpointsExceptEntry_DeletesIndexCheckpoints()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);
        var checkpointEntry = new CheckpointEntry();
        var indexCheckpointTokens = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        checkpointStore.PurgeAllCheckpointsExceptEntry(checkpointEntry);

        // Assert
        // TODO: Verify that index checkpoints are deleted
    }
}
