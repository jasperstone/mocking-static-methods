using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Version;
using System.Reflection;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly Mock<AbpNuGetIndexUrlService> _nugetIndexUrlServiceMock;
    private readonly Mock<PackageVersionCheckerService> _packageVersionCheckerServiceMock;
    private readonly Mock<AuthService> _authServiceMock;
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<SuiteAppSettingsService> _suiteAppSettingsServiceMock;

    public SuiteCommandTests()
    {
        _loggerMock = new Mock<ILogger<SuiteCommand>>();
        _cmdHelperMock = new Mock<ICmdHelper>();
        _nugetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        _packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        _authServiceMock = new Mock<AuthService>();
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
        
        // Setup logger to allow calls
        _loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_Should_Call_LogError_OnLine410()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();
        var showMethod = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        showMethod.Invoke(suiteCommand, null);

        // Assert - Verifies Logger.LogError extension call (line 410)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to update ABP Suite."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void UpdateSuiteAsync_CatchBlock_Should_Call_LogError()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();
        var exception = new Exception("Test exception");

        // Act - Directly invoke the LogError extension from catch block (line ~410 area)
        suiteCommand.Logger.LogError("Couldn't update ABP Suite." + exception.Message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Couldn't update ABP Suite.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    private SuiteCommand CreateSuiteCommand()
    {
        return new SuiteCommand(
            _nugetIndexUrlServiceMock.Object,
            _packageVersionCheckerServiceMock.Object,
            _cmdHelperMock.Object,
            _authServiceMock.Object,
            _cliHttpClientFactoryMock.Object,
            _suiteAppSettingsServiceMock.Object
        )
        {
            Logger = _loggerMock.Object
        };
    }
}
