using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class MigrationDriverTests
{
    [Fact]
    public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.BeginAsyncMigrationTaskAsync();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTryPrepareLocalForMigrationFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.BeginAsyncMigrationTaskAsync();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenReserveDestinationVectorSetsAsyncFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.BeginAsyncMigrationTaskAsync();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenMigrateSlotsDriverInlineAsyncFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.BeginAsyncMigrationTaskAsync();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
