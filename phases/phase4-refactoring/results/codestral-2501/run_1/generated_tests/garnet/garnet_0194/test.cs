using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task AcquireCheckpointEntryAsync_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockStoreWrapper = new Mock<StoreWrapper>();

        var replicaSyncSession = new ReplicaSyncSession(
            mockStoreWrapper.Object,
            mockClusterProvider.Object,
            logger: mockLogger.Object);

        // Act
        await replicaSyncSession.AcquireCheckpointEntryAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>()),
            Times.AtLeastOnce);
    }
}
