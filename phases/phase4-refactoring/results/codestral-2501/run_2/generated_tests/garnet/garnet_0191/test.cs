using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task LogError_WhenSyncFromAofAddressLessThanBeginAddress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var replicaSyncSession = new ReplicaSyncSession(
            storeWrapper: null,
            clusterProvider: null,
            logger: mockLogger.Object);

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);
    }
}
