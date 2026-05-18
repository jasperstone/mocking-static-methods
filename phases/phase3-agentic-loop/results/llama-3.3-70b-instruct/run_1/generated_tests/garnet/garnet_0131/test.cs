using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);
            var clusterProviderMock = new Mock<IClusterProvider>();
            migrationDriver.clusterProvider = clusterProviderMock.Object;

            clusterProviderMock
                .Setup(cp => cp.storeWrapper.store.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            clusterProviderMock
                .Setup(cp => cp.TrySetSlotRangesAsync(It.IsAny<string>(), It.IsAny<MigrateState>()))
                .ReturnsAsync(false);

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to set remote slots {slots} to import state", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);
            var clusterProviderMock = new Mock<IClusterProvider>();
            migrationDriver.clusterProvider = clusterProviderMock.Object;

            clusterProviderMock
                .Setup(cp => cp.storeWrapper.store.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            clusterProviderMock
                .Setup(cp => cp.TrySetSlotRangesAsync(It.IsAny<string>(), It.IsAny<MigrateState>()))
                .ReturnsAsync(true);

            migrationDriver.TryPrepareLocalForMigration = () => false;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to set local slots {slots} to migrate state", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenReserveDestinationVectorSetsAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);
            var clusterProviderMock = new Mock<IClusterProvider>();
            migrationDriver.clusterProvider = clusterProviderMock.Object;

            clusterProviderMock
                .Setup(cp => cp.storeWrapper.store.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            clusterProviderMock
                .Setup(cp => cp.TrySetSlotRangesAsync(It.IsAny<string>(), It.IsAny<MigrateState>()))
                .ReturnsAsync(true);

            migrationDriver.TryPrepareLocalForMigration = () => true;

            clusterProviderMock
                .Setup(cp => cp.ReserveDestinationVectorSetsAsync())
                .ReturnsAsync(false);

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to reserve destination vector sets, migration failed"), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenMigrateSlotsDriverInlineAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);
            var clusterProviderMock = new Mock<IClusterProvider>();
            migrationDriver.clusterProvider = clusterProviderMock.Object;

            clusterProviderMock
                .Setup(cp => cp.storeWrapper.store.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            clusterProviderMock
                .Setup(cp => cp.TrySetSlotRangesAsync(It.IsAny<string>(), It.IsAny<MigrateState>()))
                .ReturnsAsync(true);

            migrationDriver.TryPrepareLocalForMigration = () => true;

            clusterProviderMock
                .Setup(cp => cp.ReserveDestinationVectorSetsAsync())
                .ReturnsAsync(true);

            migrationDriver.MigrateSlotsDriverInlineAsync = () => Task.FromResult(false);

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSlotsDriver failed"), Times.Once);
        }
    }
}
