using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task LogError_Called_When_SetSlotRangesAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);
            var nodeid = "nodeid";
            var state = MigrateState.IMPORT;

            // Act
            var result = await migrationDriver.TrySetSlotRangesAsync(nodeid, state);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_TryPrepareLocalForMigration_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);

            // Act
            var result = migrationDriver.TryPrepareLocalForMigration();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_ReserveDestinationVectorSetsAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);

            // Act
            var result = await migrationDriver.ReserveDestinationVectorSetsAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_MigrateSlotsDriverInlineAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);

            // Act
            var result = await migrationDriver.MigrateSlotsDriverInlineAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
