using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenLocalSlotsCannotBeSetToMigrateState()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrateSession(loggerMock.Object);
            migrationDriver.TryPrepareLocalForMigration = () => false; // Simulate failure

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Failed to set local slots")),
                    It.IsAny<string[]>()),
                Times.Once);
        }
    }
}
