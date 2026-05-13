using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Garnet.cluster
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsErrorWhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            var trySetSlotRangesAsyncMock = new Mock<Func<string, MigrateState, Task<bool>>>();
            trySetSlotRangesAsyncMock.Setup(f => f(It.IsAny<string>(), It.IsAny<MigrateState>())).ReturnsAsync(false);
            migrationDriver.TrySetSlotRangesAsync = trySetSlotRangesAsyncMock.Object;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to set remote slots {slots} to import state", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsErrorWhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            var tryPrepareLocalForMigrationMock = new Mock<Func<bool>>();
            tryPrepareLocalForMigrationMock.Setup(f => f()).Returns(false);
            migrationDriver.TryPrepareLocalForMigration = tryPrepareLocalForMigrationMock.Object;
            var trySetSlotRangesAsyncMock = new Mock<Func<string, MigrateState, Task<bool>>>();
            trySetSlotRangesAsyncMock.Setup(f => f(It.IsAny<string>(), It.IsAny<MigrateState>())).ReturnsAsync(true);
            migrationDriver.TrySetSlotRangesAsync = trySetSlotRangesAsyncMock.Object;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to set local slots {slots} to migrate state", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsErrorWhenReserveDestinationVectorSetsAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            var reserveDestinationVectorSetsAsyncMock = new Mock<Func<Task<bool>>>();
            reserveDestinationVectorSetsAsyncMock.Setup(f => f()).ReturnsAsync(false);
            migrationDriver.ReserveDestinationVectorSetsAsync = reserveDestinationVectorSetsAsyncMock.Object;
            var trySetSlotRangesAsyncMock = new Mock<Func<string, MigrateState, Task<bool>>>();
            trySetSlotRangesAsyncMock.Setup(f => f(It.IsAny<string>(), It.IsAny<MigrateState>())).ReturnsAsync(true);
            migrationDriver.TrySetSlotRangesAsync = trySetSlotRangesAsyncMock.Object;
            var tryPrepareLocalForMigrationMock = new Mock<Func<bool>>();
            tryPrepareLocalForMigrationMock.Setup(f => f()).Returns(true);
            migrationDriver.TryPrepareLocalForMigration = tryPrepareLocalForMigrationMock.Object;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to reserve destination vector sets, migration failed"), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsErrorWhenMigrateSlotsDriverInlineAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            var migrateSlotsDriverInlineAsyncMock = new Mock<Func<Task<bool>>>();
            migrateSlotsDriverInlineAsyncMock.Setup(f => f()).ReturnsAsync(false);
            migrationDriver.MigrateSlotsDriverInlineAsync = migrateSlotsDriverInlineAsyncMock.Object;
            var trySetSlotRangesAsyncMock = new Mock<Func<string, MigrateState, Task<bool>>>();
            trySetSlotRangesAsyncMock.Setup(f => f(It.IsAny<string>(), It.IsAny<MigrateState>())).ReturnsAsync(true);
            migrationDriver.TrySetSlotRangesAsync = trySetSlotRangesAsyncMock.Object;
            var tryPrepareLocalForMigrationMock = new Mock<Func<bool>>();
            tryPrepareLocalForMigrationMock.Setup(f => f()).Returns(true);
            migrationDriver.TryPrepareLocalForMigration = tryPrepareLocalForMigrationMock.Object;
            var reserveDestinationVectorSetsAsyncMock = new Mock<Func<Task<bool>>>();
            reserveDestinationVectorSetsAsyncMock.Setup(f => f()).ReturnsAsync(true);
            migrationDriver.ReserveDestinationVectorSetsAsync = reserveDestinationVectorSetsAsyncMock.Object;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSlotsDriver failed"), Times.Once);
        }
    }
}
