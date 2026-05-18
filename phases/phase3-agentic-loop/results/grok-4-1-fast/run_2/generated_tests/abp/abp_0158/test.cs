using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using Xunit;
using NuGet.Versioning;
using Volo.Abp.Cli;
using System.Linq;

namespace Volo.Abp.Cli.Tests;

public class CliServiceTests
{
    private readonly Mock<ILogger<CliService>> _loggerMock;

    public CliServiceTests()
    {
        _loggerMock = new Mock<ILogger<CliService>>();
        _loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [Fact]
    public void LogNewVersionInfo_ShouldLogWarningForStableChannel()
    {
        // Arrange
        var cliService = new CliServiceTestable();
        cliService.Logger = _loggerMock.Object;
        var toolPath = @"C:\some\path";
        var latestVersion = new SemanticVersion(1, 2, 3);

        // Act
        cliService.CallLogNewVersionInfo(CliService.UpdateChannel.Stable, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 1.2.3"))),
            Times.Once());
        _loggerMock.Verify(
            x => x.LogWarning("dotnet tool update --tool-path C:\\some\\path Volo.Abp.Cli"),
            Times.Once());
    }

    [Fact]
    public void LogNewVersionInfo_ShouldLogWarningForPrereleaseChannel()
    {
        // Arrange
        var cliService = new CliServiceTestable();
        cliService.Logger = _loggerMock.Object;
        var toolPath = @"C:\some\path";
        var latestVersion = new SemanticVersion(1, 2, 3, "alpha");

        // Act
        cliService.CallLogNewVersionInfo(CliService.UpdateChannel.Prerelease, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("A newer prerelease version of the ABP CLI is available: 1.2.3-alpha"))),
            Times.Once());
        _loggerMock.Verify(
            x => x.LogWarning("dotnet tool update --tool-path C:\\some\\path Volo.Abp.Cli --version 1.2.3-alpha"),
            Times.Once());
    }

    [Fact]
    public void LogNewVersionInfo_ShouldLogWarningForGlobalTool()
    {
        // Arrange
        var cliService = new CliServiceTestable();
        cliService.Logger = _loggerMock.Object;
        var toolPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\.dotnet\\tools\\");
        var latestVersion = new SemanticVersion(1, 2, 3);

        // Act
        cliService.CallLogNewVersionInfo(CliService.UpdateChannel.Stable, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning("dotnet tool update -g Volo.Abp.Cli"),
            Times.Once());
    }

    [Fact]
    public void LogNewVersionInfo_ShouldLogWarningForNightlyChannel()
    {
        // Arrange
        var cliService = new CliServiceTestable();
        cliService.Logger = _loggerMock.Object;
        var toolPath = @"C:\some\path";
        var latestVersion = new SemanticVersion(1, 2, 3);

        // Act
        cliService.CallLogNewVersionInfo(CliService.UpdateChannel.Nightly, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning("dotnet tool uninstall --tool-path C:\\some\\path Volo.Abp.Cli"),
            Times.Once());
        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool install --tool-path C:\\some\\path Volo.Abp.Cli --add-source"))),
            Times.Once());
    }
}

public class CliServiceTestable : CliService
{
    public new ILogger<CliService> Logger { get; set; } = NullLogger<CliService>.Instance;

    public void CallLogNewVersionInfo(UpdateChannel updateChannel, SemanticVersion latestVersion, string toolPath, string message = null)
    {
        LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);
    }

    public new bool IsGlobalTool(string toolPath)
    {
        var globalPaths = new[] { @"%USERPROFILE%\.dotnet\tools\", "%HOME%/.dotnet/tools/" };
        return globalPaths.Select(Environment.ExpandEnvironmentVariables).Contains(toolPath);
    }
}
