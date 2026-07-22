using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NuGet.Versioning;
using System;
using System.Reflection;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests;

public class CliServiceTests
{
    private readonly Mock<ILogger<CliService>> _mockLogger;
    private readonly CliService _cliService;
    private readonly MethodInfo _logNewVersionInfoMethod;

    public CliServiceTests()
    {
        _mockLogger = new Mock<ILogger<CliService>>();

        _cliService = new CliService(
            commandLineArgumentParser: null!,
            commandSelector: null!,
            serviceScopeFactory: null!,
            nugetService: null!,
            cmdHelper: null!,
            memoryService: null!,
            cliVersionService: null!,
            telemetryService: null!)
        {
            Logger = _mockLogger.Object
        };

        _logNewVersionInfoMethod = typeof(CliService).GetMethod("LogNewVersionInfo", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectUpdateCommand()
    {
        // Arrange
        var latestVersion = new SemanticVersion(99, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        InvokeLogNewVersionInfo(0, latestVersion, toolPath); // 0 = Stable

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(
                It.Is<string>(msg => msg.Contains("stable") && msg.Contains("99.0.0")),
                It.IsAny<Exception>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.LogWarning("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli"),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsVersionSpecificUpdateCommand()
    {
        // Arrange
        var latestVersion = new SemanticVersion(7, 3, 0, "alpha", "1");
        var toolPath = @"C:\Users\user\.dotnet\tools\abp";

        // Act
        InvokeLogNewVersionInfo(2, latestVersion, toolPath); // 2 = Prerelease

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning("dotnet tool update -g Volo.Abp.Cli --version 7.3.0-alpha.1"),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_LogsUninstallAndInstallCommands()
    {
        // Arrange
        var latestVersion = new SemanticVersion(999, 9, 9);
        var toolPath = @"C:\local\tool";

        // Act
        InvokeLogNewVersionInfo(3, latestVersion, toolPath); // 3 = Nightly

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning("dotnet tool uninstall --tool-path C:\\local\\tool Volo.Abp.Cli"),
            Times.Once);

        _mockLogger.Verify(
            x => x.LogWarning(
                It.Is<string>(msg => msg.Contains("dotnet tool install") && 
                                   msg.Contains("abp-nightly") &&
                                   msg.Contains("999.9.9"))),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_WithCustomMessage_LogsTheMessage()
    {
        // Arrange
        var latestVersion = new SemanticVersion(1, 2, 3);
        var toolPath = @"global-tool-path";
        var customMessage = "This is a custom warning message!";

        // Act
        InvokeLogNewVersionInfo(0, latestVersion, toolPath, customMessage); // 0 = Stable

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(customMessage),
            Times.Once);
    }

    private void InvokeLogNewVersionInfo(int updateChannelInt, SemanticVersion latestVersion, string toolPath, string message = null)
    {
        var updateChannel = (Enum)Enum.ToObject(typeof(CliService.UpdateChannel), updateChannelInt);
        _logNewVersionInfoMethod.Invoke(_cliService, new object?[] { updateChannel, latestVersion, toolPath, message });
    }
}
