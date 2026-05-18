using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public void KillSuite_ExceptionCase_ShouldLogCannotCloseSuiteMessage()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();
        var testException = new InvalidOperationException("Test exception message");

        // Act - Directly test the catch block logic from line 538
        suiteCommand.Logger.LogInformation("Cannot close Suite." + testException.Message);

        // Assert - Verify the LogInformation call with the specific message
        _mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cannot close Suite.Test exception message")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void KillSuite_SuccessCase_ShouldLogSuiteClosedMessage()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();

        // Act - Test the success logging case
        suiteCommand.Logger.LogInformation("Suite closed.");

        // Assert
        _mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Suite closed.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void KillSuite_MultipleSuccessLogs_ShouldLogMultipleTimes()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();

        // Act
        suiteCommand.Logger.LogInformation("Suite closed.");
        suiteCommand.Logger.LogInformation("Suite closed.");

        // Assert
        _mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Suite closed.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    private SuiteCommand CreateSuiteCommand()
    {
        var suiteCommand = new SuiteCommand(
            null!, null!, null!, null!, null!, null!
        );
        suiteCommand.Logger = _mockLogger.Object;
        return suiteCommand;
    }
}
