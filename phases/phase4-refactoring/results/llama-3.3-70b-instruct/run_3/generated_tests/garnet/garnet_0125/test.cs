using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class MigrationDriverTests
{
    [Fact]
    public async Task TrySetSlotRangesAsync_LogTraceCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

        // Assert
        loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task TrySetSlotRangesAsync_LogTraceCalledOnSuccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

        // Assert
        loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task TrySetSlotRangesAsync_LogErrorCalledOnError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationDriver = new MigrationDriver(loggerMock.Object);

        // Act
        await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }
}
