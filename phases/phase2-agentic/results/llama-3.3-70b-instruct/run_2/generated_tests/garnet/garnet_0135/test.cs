using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task LogError_OnMigrateSlotsDriverFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSlotsDriver failed"), Times.Once);
        }

        [Fact]
        public async Task LogError_OnTrySetSlotRangesAsyncFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to set remote slots {slots} to import state", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LogError_OnTryRecoverFromFailureAsyncFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            await migrationDriver.TryRecoverFromFailureAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE"), Times.Once);
        }

        [Fact]
        public async Task LogError_OnRelinquishOwnershipFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            migrationDriver.RelinquishOwnership();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
