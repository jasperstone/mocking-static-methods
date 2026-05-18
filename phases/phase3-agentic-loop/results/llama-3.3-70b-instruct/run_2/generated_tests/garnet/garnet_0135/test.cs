using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task LogError_Called_When_MigrateSlotsDriverInlineAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null, null);

            // Act
            await migrationSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSlotsDriver failed"), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_TrySetSlotRangesAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null, null);

            // Act
            await migrationSession.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to set remote slots {slots} to import state", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_TryRecoverFromFailureAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null, null);

            // Act
            await migrationSession.TryRecoverFromFailureAsync();

            // Assert
            loggerMock.Verify(l => l.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE"), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_ReliquishOwnership_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object, null, null, null, null, null, null, null);

            // Act
            migrationSession.RelinquishOwnership();

            // Assert
            loggerMock.Verify(l => l.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
