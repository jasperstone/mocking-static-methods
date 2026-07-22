using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class MigrationDriverTests
{
    [Fact]
    public async Task LogError_Called_When_MigrateSlotsDriver_Fails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.MigrateSlotsDriverInlineAsync();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LogError_Called_When_TrySetSlotRangesAsync_Fails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.TrySetSlotRangesAsync("nodeId", 0);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LogError_Called_When_TryRecoverFromFailureAsync_Fails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.TryRecoverFromFailureAsync();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
    }
}
