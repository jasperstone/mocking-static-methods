using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

public class MigrationDriverTests
{
    [Fact]
    public async Task BeginAsyncMigrationTaskAsync_RelinquishOwnershipFails_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateSession>>();
        var mockClusterProvider = new Mock<IClusterProvider>();
        var mockClusterManager = new Mock<IClusterManager>();
        var mockStoreWrapper = new Mock<IStoreWrapper>();
        var mockStore = new Mock<IStore>();
        var mockMigrationManager = new Mock<IMigrationManager>();
        var mockMigrateOperation = new Mock<IMigrateOperation>();

        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
        mockStoreWrapper.Setup(sw => sw.store).Returns(mockStore.Object);
        mockClusterProvider.Setup(cp => cp.migrationManager).Returns(mockMigrationManager.Object);

        var migrateSession = new MigrateSession(
            mockClusterProvider.Object,
            mockMigrateOperation.Object,
            mockLogger.Object
        );

        // Mock the failure of RelinquishOwnership
        migrateSession.RelinquishOwnership = () => false;

        // Act
        await migrateSession.BeginAsyncMigrationTaskAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
