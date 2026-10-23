using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task AcquireCheckpointEntryAsync_LogsInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<Garnet.common.StoreWrapper>();
        var clusterProviderMock = new Mock<Garnet.cluster.ClusterProvider>();
        var replicaSyncSession = new Garnet.cluster.ReplicaSyncSession(storeWrapperMock.Object, clusterProviderMock.Object, null, default, null, null, null, loggerMock.Object);

        // Act
        await replicaSyncSession.AcquireCheckpointEntryAsync();

        // Assert
        loggerMock.Verify(l => l.LogInformation("AcquireCheckpointEntry iteration {iteration}", 1), Times.Once);
    }
}
