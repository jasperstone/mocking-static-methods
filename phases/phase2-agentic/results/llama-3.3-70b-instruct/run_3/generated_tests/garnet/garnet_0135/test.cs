using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Garnet.cluster
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task MigrateSlotsDriverInlineAsync_LogsError_WhenFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            await migrationDriver.MigrateSlotsDriverInlineAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSlotsDriver failed"), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError("SetSlotRange error: {error}", "error"), Times.Once);
        }

        [Fact]
        public async Task TryRecoverFromFailureAsync_LogsError_WhenFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            await migrationDriver.TryRecoverFromFailureAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE"), Times.Once);
        }

        [Fact]
        public async Task RelinquishOwnership_LogsError_WhenFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrationDriver>>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Act
            migrationDriver.RelinquishOwnership();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", "srcNode", "tgtNode"), Times.Once);
        }
    }
}
