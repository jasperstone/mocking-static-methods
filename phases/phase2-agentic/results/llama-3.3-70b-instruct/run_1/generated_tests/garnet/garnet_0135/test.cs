using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Garnet.cluster
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenMigrateSlotsDriverFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            migrationDriver.MigrateSlotsDriverInlineAsync = async () => false;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSlotsDriver failed"), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            migrationDriver.TrySetSlotRangesAsync = async (nodeId, state) => false;

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
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            migrationDriver.TryPrepareLocalForMigration = () => false;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to set local slots {slots} to migrate state", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenRelinquishOwnershipFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);
            migrationDriver.RelinquishOwnership = () => false;

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
