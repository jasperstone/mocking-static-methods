using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly SuiteCommand _suiteCommand;
    private readonly MethodInfo _showSuiteManualUpdateMethod;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        _mockCmdHelper = new Mock<ICmdHelper>();
        
        _suiteCommand = new SuiteCommand(
            nuGetIndexUrlService: Mock.Of<AbpNuGetIndexUrlService>(),
            packageVersionCheckerService: null!,
            cmdHelper: _mockCmdHelper.Object,
            authService: Mock.Of<AuthService>(),
            cliHttpClientFactory: Mock.Of<CliHttpClientFactory>(),
            suiteAppSettingsService: Mock.Of<SuiteAppSettingsService>()
        );

        // Set the logger (public property)
        _suiteCommand.Logger = _mockLogger.Object;

        // Get the private method
        _showSuiteManualUpdateMethod = typeof(SuiteCommand)
            .GetMethod("ShowSuiteManualUpdateCommand", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_Should_LogErrorMessages()
    {
        // Act
        _showSuiteManualUpdateMethod.Invoke(_suiteCommand, null);

        // Assert - Verify first LogError call (line ~410)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("You can also run the following command to update ABP Suite.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );

        // Assert - Verify second LogError call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_Verify_LogError_Called_Twice()
    {
        // Act
        _showSuiteManualUpdateMethod.Invoke(_suiteCommand, null);

        // Assert - Verify LogError was called exactly twice
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
            Times.Exactly(2));
    }
}
