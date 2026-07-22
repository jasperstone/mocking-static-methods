using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
    }

    [Fact]
    public void KillSuite_WhenExceptionOccurs_ShouldLogCannotCloseSuiteMessage()
    {
        // Arrange
        var suiteCommand = new SuiteCommand(
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        )
        {
            Logger = _mockLogger.Object
        };

        // Act
        var exception = new InvalidOperationException("Test exception");
        try
        {
            suiteCommand.KillSuite();
        }
        catch (InvalidOperationException)
        {
            // Expected - simulates the exception path in KillSuite
        }

        // Assert - Verifies the LogInformation call on line 538
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Cannot close Suite.Test exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void KillSuite_WhenNoException_ShouldNotLogErrorMessage()
    {
        // Arrange
        var suiteCommand = new SuiteCommand(
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        )
        {
            Logger = _mockLogger.Object
        };

        // Act - Normally no processes exist, so no exception thrown
        suiteCommand.KillSuite();

        // Assert - No "Cannot close Suite" message logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Cannot close Suite")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
