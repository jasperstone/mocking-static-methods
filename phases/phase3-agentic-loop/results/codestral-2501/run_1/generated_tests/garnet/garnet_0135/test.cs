using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System;

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

        var migrateSession = new MigrateSession(mockLogger.Object, mockClusterProvider.Object);

        // Mock the failure of RelinquishOwnership
        migrateSession.RelinquishOwnership = () => false;

        // Act
        await migrateSession.BeginAsyncMigrationTaskAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                "Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})",
                It.IsAny<object>(),
                It.IsAny<object>()),
            Times.Once);
    }
}
