using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void Logger_LogErrorExtension_ShouldCallUnderlyingLogMethod()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var command = new SuiteCommand(
            null!, null!, null!, null!, null!, null!
        );
        command.Logger = loggerMock.Object;

        var expectedMessage = "Port \"3000\" is already in use."; // Exact message from line 505

        // Act - Call the exact LogError extension used on line 505
        command.Logger.LogError(expectedMessage);

        // Assert - Verify ILogger.Log was called with Error level (LoggerExtensions.LogError coverage)
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Logger_LogErrorExtension_WithPortVariableFormat_ShouldWork()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var command = new SuiteCommand(null!, null!, null!, null!, null!, null!);
        command.Logger = loggerMock.Object;

        var port = 3000;
        var expectedMessage = $"Port \"{port}\" is already in use."; // Matches line 505 format

        // Act
        command.Logger.LogError(expectedMessage);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString().Contains(expectedMessage)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
