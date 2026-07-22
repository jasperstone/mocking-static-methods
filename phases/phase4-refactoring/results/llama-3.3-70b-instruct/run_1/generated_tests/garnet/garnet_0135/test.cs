using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class MigrationSessionTests
    {
        [Fact]
        public async Task LogError_Called_When_MigrateSlotsDriverInlineAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object);

            // Act
            await migrationSession.MigrateSlotsDriverInlineAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_TrySetSlotRangesAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object);

            // Act
            await migrationSession.TrySetSlotRangesAsync("nodeId", MigrateState.NODE);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_TryRecoverFromFailureAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationSession = new MigrateSession(loggerMock.Object);

            // Act
            await migrationSession.TryRecoverFromFailureAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
