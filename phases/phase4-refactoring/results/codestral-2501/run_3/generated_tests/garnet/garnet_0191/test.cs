using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Threading;
using System;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task LogError_WhenSyncFromAofAddressLessThanBeginAddress_ShouldLogError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var storeWrapper = new StoreWrapper();
        var clusterProvider = new ClusterProvider();
        var replicaSyncSession = new ReplicaSyncSession(storeWrapper, clusterProvider, logger: mockLogger.Object);

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
