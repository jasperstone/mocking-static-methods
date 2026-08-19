using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        _mockLogger.SetupAllProperties();
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_Should_LogError_YouCanAlsoRunMessage()
    {
        // Arrange
        var suiteCommand = CreateMinimalSuiteCommand();

        // Act
        suiteCommand.ShowSuiteManualUpdateCommand();

        // Assert - Verifies the Logger.LogError call on line 410
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("You can also run the following command to update ABP Suite.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_Should_LogError_DotnetCommand()
    {
        // Arrange
        var suiteCommand = CreateMinimalSuiteCommand();

        // Act
        suiteCommand.ShowSuiteManualUpdateCommand();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool update -g Volo.Abp.Suite")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private SuiteCommand CreateMinimalSuiteCommand()
    {
        // Use NullLogger and minimal object initialization to avoid dependency issues
        return new SuiteCommand(
            nugetIndexUrlService: null!,
            packageVersionCheckerService: null!,
            cmdHelper: null!,
            authService: null!,
            cliHttpClientFactory: null!,
            suiteAppSettingsService: null!
        )
        {
            Logger = _mockLogger.Object
        };
    }
}
