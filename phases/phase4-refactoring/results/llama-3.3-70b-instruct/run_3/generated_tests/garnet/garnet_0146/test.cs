using Xunit;
using Moq;
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
        var entry = new CheckpointEntry();

        // Act
        checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

        // Assert
        loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public void PurgeAllCheckpointsExceptEntry_DeletesLogCheckpoints()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);
        var entry = new CheckpointEntry();
        var ckptManagerMock = new Mock<IReplicationLogCheckpointManager>();
        ckptManagerMock.Setup(m => m.GetLogCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });
        ckptManagerMock.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>())).Verifiable();

        // Act
        checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

        // Assert
        ckptManagerMock.Verify(m => m.DeleteLogCheckpoint(It.IsAny<Guid>()), Times.AtLeastOnce);
    }

    [Fact]
    public void PurgeAllCheckpointsExceptEntry_DeletesIndexCheckpoints()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var clusterProviderMock = new Mock<ClusterProvider>();
        var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);
        var entry = new CheckpointEntry();
        var ckptManagerMock = new Mock<IReplicationLogCheckpointManager>();
        ckptManagerMock.Setup(m => m.GetIndexCheckpointTokens()).Returns(new[] { Guid.NewGuid(), Guid.NewGuid() });
        ckptManagerMock.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>())).Verifiable();

        // Act
        checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

        // Assert
        ckptManagerMock.Verify(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>()), Times.AtLeastOnce);
    }
}
