using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogError_When_TryPrepareLocalForMigration_Fails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockStore = new Mock<IStore>();
            var mockCancellationTokenSource = new Mock<CancellationTokenSource>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockVectorManager = new Mock<IVectorManager>();
            var mockDatabase = new Mock<IDatabase>();
            var mockMigrationManager = new Mock<IMigrationManager>();

            mockStoreWrapper.Setup(sw => sw.store).Returns(mockStore.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.migrationManager).Returns(mockMigrationManager.Object);
            mockDatabase.Setup(db => db.VectorManager).Returns(mockVectorManager.Object);
            mockStoreWrapper.Setup(sw => sw.DefaultDatabase).Returns(mockDatabase.Object);

            var migrationDriver = new MigrateSession(mockClusterProvider.Object, mockLogger.Object, mockCancellationTokenSource.Object);

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
