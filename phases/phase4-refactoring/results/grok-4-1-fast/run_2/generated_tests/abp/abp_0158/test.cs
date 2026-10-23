using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Cli.Version;
using Xunit;
using NuGet.Versioning;

namespace Volo.Abp.Cli.Tests;

public class CliServiceTests
{
    private readonly Mock<ILogger<CliService>> _loggerMock;
    private readonly CliService _cliService;

    public CliServiceTests()
    {
        _loggerMock = new Mock<ILogger<CliService>>();
        _loggerMock.SetupAllProperties();

        _cliService = new CliService(
            new Mock<ICommandLineArgumentParser>().Object,
            new Mock<ICommandSelector>().Object,
            new Mock<IServiceScopeFactory>().Object,
            new Mock<PackageVersionCheckerService>().Object,
            new Mock<ICmdHelper>().Object,
            new Mock<MemoryService>().Object,
            new Mock<CliVersionService>().Object,
            new Mock<ITelemetryService>().Object
        )
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public void LogNewVersionInfo_StableChannel_ShouldLogCorrectUpdateCommand()
    {
        // Arrange
        var updateChannel = CliService.UpdateChannel.Stable;
        var latestVersion = new SemanticVersion(99, 0, 0);
        var toolPath = @"C:\local\tool";

        // Act
        _cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(msg => msg.Contains("stable") && msg.Contains("99.0.0")), Times.Once);
        _loggerMock.Verify(x => x.LogWarning("Update Command: "), Times.Once);
        _loggerMock.Verify(x => x.LogWarning("dotnet tool update --tool-path C:\\local\\tool Volo.Abp.Cli"), Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_ShouldLogVersionSpecificUpdate()
    {
        // Arrange
        var updateChannel = CliService.UpdateChannel.Prerelease;
        var latestVersion = new SemanticVersion(7, 3, 0, "preview.1");
        var toolPath = @"%USERPROFILE%\.dotnet\tools\tool";

        // Act
        _cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(msg => msg.Contains("prerelease") && msg.Contains("7.3.0-preview.1")), Times.Once);
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(msg => msg.Contains("dotnet tool update -g Volo.Abp.Cli --version 7.3.0-preview.1")), Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_ShouldLogUninstallAndInstallCommands()
    {
        // Arrange
        var updateChannel = CliService.UpdateChannel.Nightly;
        var latestVersion = new SemanticVersion(999, 9, 9, "nightly");
        var toolPath = @"C:\local\tool";

        // Act
        _cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(msg => msg.Contains("nightly") && msg.Contains("999.9.9-nightly")), Times.Once);
        _loggerMock.Verify(x => x.LogWarning("dotnet tool uninstall --tool-path C:\\local\\tool Volo.Abp.Cli"), Times.Once);
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(msg => msg.Contains("dotnet tool install") && msg.Contains("abp-nightly")), Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_WithMessage_ShouldLogCustomMessage()
    {
        // Arrange
        var updateChannel = CliService.UpdateChannel.Stable;
        var latestVersion = new SemanticVersion(1, 2, 3);
        var toolPath = "/home/user/.dotnet/tools/tool";
        var customMessage = "Custom update message here!";

        // Act
        _cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, customMessage);

        // Assert
        _loggerMock.Verify(x => x.LogWarning(customMessage), Times.Once);
    }

    [Fact]
    public void IsGlobalTool_GlobalPath_ShouldReturnTrue()
    {
        Assert.True(_cliService.IsGlobalTool(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.dotnet\tools\")));
        Assert.True(_cliService.IsGlobalTool(Environment.ExpandEnvironmentVariables(@"%HOME%/.dotnet/tools/")));
    }

    [Fact]
    public void IsGlobalTool_LocalPath_ShouldReturnFalse()
    {
        Assert.False(_cliService.IsGlobalTool(@"C:\local\tool"));
        Assert.False(_cliService.IsGlobalTool(@"/usr/local/tool"));
    }
}
