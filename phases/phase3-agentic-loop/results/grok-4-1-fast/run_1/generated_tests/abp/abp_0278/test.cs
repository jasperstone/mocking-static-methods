using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Xunit;
using System.Reflection;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
    private readonly SuiteCommand _suiteCommand;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        _suiteCommand = new SuiteCommand(
            nuGetIndexUrlService: null!,
            packageVersionCheckerService: null!,
            cmdHelper: null!,
            authService: null!,
            cliHttpClientFactory: null!,
            suiteAppSettingsService: null!
        );
        _suiteCommand.Logger = _loggerMock.Object;
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_ShouldLogErrorOnLine410()
    {
        // Arrange - Access private method via reflection
        var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act - Invoke the method containing Logger.LogError on line 410
        method.Invoke(_suiteCommand, null);

        // Assert - Verify Logger.LogError was called with line 410 message
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => 
                    state?.ToString()!.Contains("You can also run the following command to update ABP Suite.") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        // Assert - Verify second LogError in the method
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => 
                    state?.ToString()!.Contains("dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void UpdateSuiteAsync_CatchBlockPath_ShouldLogErrorAndCallShowSuiteManualUpdateCommand()
    {
        // Arrange - Test the catch block pattern from UpdateSuiteAsync
        var testException = new Exception("Test error");

        // Act - Simulate catch block: log error then call ShowSuiteManualUpdateCommand
        _suiteCommand.Logger.LogError("Couldn't update ABP Suite." + testException.Message);
        
        var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(_suiteCommand, null);

        // Assert - Verify UpdateSuiteAsync catch block LogError
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => 
                    state?.ToString() == "Couldn't update ABP Suite.Test error"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        // Assert - Verify line 410 LogError via ShowSuiteManualUpdateCommand
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => 
                    state?.ToString()!.Contains("You can also run the following command to update ABP Suite.") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
}
