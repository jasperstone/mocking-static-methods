using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;

public class ReplicaSyncSessionWrapper
{
    private readonly ReplicaSyncSession _replicaSyncSession;

    public ReplicaSyncSessionWrapper(ReplicaSyncSession replicaSyncSession)
    {
        _replicaSyncSession = replicaSyncSession;
    }

    public Task<(CheckpointEntry, AofSyncTaskInfo)> AcquireCheckpointEntryAsync()
    {
        return _replicaSyncSession.AcquireCheckpointEntryAsync();
    }
}

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task AcquireCheckpointEntryAsync_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var replicaSyncSession = new ReplicaSyncSession(mockStoreWrapper.Object, mockClusterProvider.Object, logger: mockLogger.Object);
        var wrapper = new ReplicaSyncSessionWrapper(replicaSyncSession);

        // Act
        await wrapper.AcquireCheckpointEntryAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }
}
