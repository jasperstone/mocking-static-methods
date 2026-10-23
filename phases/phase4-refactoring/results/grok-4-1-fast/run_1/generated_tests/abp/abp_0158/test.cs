using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using System;
using System.Reflection;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests;

public class CliServiceTests
{
    private readonly Mock<ILogger<CliService>> _loggerMock;
    private readonly CliService _cliService;

    public CliServiceTests()
    {
        _loggerMock = new Mock<ILogger<CliService>>();

        // Create mocks for all required dependencies
        var commandLineArgumentParserMock = new Mock<ICommandLineArgumentParser>();
        var commandSelectorMock = new Mock<ICommandSelector>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var memoryServiceMock = new Mock<MemoryService>();
        var cliVersionServiceMock = new Mock<CliVersionService>();
        var telemetryServiceMock = new Mock<ITelemetryService>();

        _cliService = new CliService(
            commandLineArgumentParserMock.Object,
            commandSelectorMock.Object,
            serviceScopeFactoryMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            memoryServiceMock.Object,
            cliVersionServiceMock.Object,
            telemetryServiceMock.Object
        )
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectUpdateCommand()
    {
        // Arrange
        var toolPath = @"C:\some\path";
        var latestVersion = new SemanticVersion(99, 0, 0);
        var updateChannelField = typeof(CliService).GetField("UpdateChannel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
        var updateChannelEnum = updateChannelField.FieldType.GetEnumValues().GetValue(1); // Stable is typically index 1

        // Act
        InvokeLogNewVersionInfo(updateChannelEnum, latestVersion, toolPath);

        // Assert - specifically verify the line 325 call (Logger.LogWarning for newer version message)
        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("A newer stable version") && s.Contains("99.0.0")),
                It.IsAny<Exception>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.LogWarning("dotnet tool update --tool-path C:\\some\\path Volo.Abp.Cli"),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsVersionSpecificUpdateCommand()
    {
        // Arrange
        var toolPath = @"C:\some\path";
        var latestVersion = new SemanticVersion(99, 0, 0);
        var updateChannelField = typeof(CliService).GetField("UpdateChannel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
        var updateChannelEnum = updateChannelField.FieldType.GetEnumValues().GetValue(2); // Prerelease

        // Act
        InvokeLogNewVersionInfo(updateChannelEnum, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning("dotnet tool update --tool-path C:\\some\\path Volo.Abp.Cli --version 99.0.0"),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_GlobalTool_UsesGlobalFlag()
    {
        // Arrange
        var toolPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.dotnet\tools\");
        var latestVersion = new SemanticVersion(99, 0, 0);
        var updateChannelField = typeof(CliService).GetField("UpdateChannel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
        var updateChannelEnum = updateChannelField.FieldType.GetEnumValues().GetValue(1); // Stable

        // Act
        InvokeLogNewVersionInfo(updateChannelEnum, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning("dotnet tool update -g Volo.Abp.Cli"),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_WithCustomMessage_LogsAdditionalMessage()
    {
        // Arrange
        var toolPath = @"C:\some\path";
        var latestVersion = new SemanticVersion(99, 0, 0);
        var customMessage = "Custom update message";
        var updateChannelField = typeof(CliService).GetField("UpdateChannel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
        var updateChannelEnum = updateChannelField.FieldType.GetEnumValues().GetValue(1); // Stable

        // Act
        InvokeLogNewVersionInfo(updateChannelEnum, latestVersion, toolPath, customMessage);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning(customMessage),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_LogsUninstallInstallCommands()
    {
        // Arrange
        var toolPath = @"C:\some\path";
        var latestVersion = new SemanticVersion(99, 0, 0);
        var updateChannelField = typeof(CliService).GetField("UpdateChannel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
        var updateChannelEnum = updateChannelField.FieldType.GetEnumValues().GetValue(3); // Nightly

        // Act
        InvokeLogNewVersionInfo(updateChannelEnum, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning("dotnet tool uninstall --tool-path C:\\some\\path Volo.Abp.Cli"),
            Times.Once);

        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool install") && 
                                  s.Contains("myget.org/F/abp-nightly") && 
                                  s.Contains("99.0.0"))),
            Times.Once);
    }

    private void InvokeLogNewVersionInfo(object updateChannel, SemanticVersion latestVersion, string toolPath, string message = null)
    {
        var method = typeof(CliService).GetMethod("LogNewVersionInfo", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        var parameters = new object[] { updateChannel, latestVersion, toolPath, message };
        method.Invoke(_cliService, parameters);
    }
}
