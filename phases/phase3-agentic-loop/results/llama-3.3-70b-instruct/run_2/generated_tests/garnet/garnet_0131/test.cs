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
            var trySetSlotRangesAsyncMock = new Mock<Func<string, MigrateState, Task<bool>>>();
            trySetSlotRangesAsyncMock.Setup(f => f(It.IsAny<string>(), It.IsAny<MigrateState>())).ReturnsAsync(false);

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
            var tryPrepareLocalForMigrationMock = new Mock<Func<bool>>();
            tryPrepareLocalForMigrationMock.Setup(f => f()).Returns(false);

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
            var reserveDestinationVectorSetsAsyncMock = new Mock<Func<Task<bool>>>();
            reserveDestinationVectorSetsAsyncMock.Setup(f => f()).ReturnsAsync(false);

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
            var migrateSlotsDriverInlineAsyncMock = new Mock<Func<Task<bool>>>();
            migrateSlotsDriverInlineAsyncMock.Setup(f => f()).ReturnsAsync(false);

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSlotsDriver failed"), Times.Once);
        }
    }
}
