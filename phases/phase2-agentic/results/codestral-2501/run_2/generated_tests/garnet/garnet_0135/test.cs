using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_RelinquishOwnershipFails_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<Store>();
            var mockMigrationManager = new Mock<MigrationManager>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(sw => sw.store).Returns(mockStore.Object);
            mockClusterProvider.Setup(cp => cp.migrationManager).Returns(mockMigrationManager.Object);

            var migrationDriver = new MigrateSession(mockLogger.Object, mockClusterProvider.Object);

            // Mock the necessary methods to simulate failure
            migrationDriver.RelinquishOwnership = () => false;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    "Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})",
                    It.IsAny<object>(),
                    It.IsAny<object>()),
                Times.Once);
        }
    }
}
