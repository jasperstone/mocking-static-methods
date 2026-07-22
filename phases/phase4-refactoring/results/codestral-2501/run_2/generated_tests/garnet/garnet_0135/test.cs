using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class MigrationDriverTests
{
    [Fact]
    public async Task BeginAsyncMigrationTaskAsync_ShouldLogErrorWhenRelinquishOwnershipFails()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateSession>>();
        var mockClusterProvider = new Mock<IClusterProvider>();
        var mockClusterManager = new Mock<IClusterManager>();
        var mockStoreWrapper = new Mock<IStoreWrapper>();
        var mockStore = new Mock<IStore>();
        var mockMigrationManager = new Mock<IMigrationManager>();

        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
        mockClusterProvider.Setup(cp => cp.migrationManager).Returns(mockMigrationManager.Object);
        mockStoreWrapper.Setup(sw => sw.store).Returns(mockStore.Object);

        var migrationDriver = new MigrateSession(
            new ClusterSession(),
            mockClusterProvider.Object,
            "targetAddress",
            1234,
            "targetNodeId",
            "username",
            "password",
            "sourceNodeId",
            false,
            false,
            1000,
            new HashSet<int> { 1, 2, 3 },
            new Sketch(),
            TransferOption.SLOTS);

        // Act
        await migrationDriver.BeginAsyncMigrationTaskAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
