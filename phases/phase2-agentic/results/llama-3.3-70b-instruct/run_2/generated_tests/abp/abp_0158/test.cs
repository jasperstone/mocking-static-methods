using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NuGet.Versioning;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Memory;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp.Cli;

public class CliServiceTests
{
    [Fact]
    public async Task LogNewVersionInfo_LogsWarning_WhenNewVersionIsAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliService(
            Mock.Of<ICommandLineArgumentParser>(),
            Mock.Of<ICommandSelector>(),
            Mock.Of<IServiceScopeFactory>(),
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<MemoryService>(),
            Mock.Of<CliVersionService>(),
            Mock.Of<ITelemetryService>()
        );
        cliService.Logger = loggerMock.Object;

        var updateChannel = UpdateChannel.Stable;
        var latestVersion = new SemanticVersion(2, 0, 0);
        var toolPath = "tool-path";
        var message = "message";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 2.0.0."))),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == message)),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == string.Empty)),
            Times.Exactly(2));
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == "Update Command: ")),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == "dotnet tool update --tool-path tool-path Volo.Abp.Cli")),
            Times.Once);
    }

    [Fact]
    public async Task LogNewVersionInfo_LogsWarning_WhenNewPrereleaseVersionIsAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliService(
            Mock.Of<ICommandLineArgumentParser>(),
            Mock.Of<ICommandSelector>(),
            Mock.Of<IServiceScopeFactory>(),
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<MemoryService>(),
            Mock.Of<CliVersionService>(),
            Mock.Of<ITelemetryService>()
        );
        cliService.Logger = loggerMock.Object;

        var updateChannel = UpdateChannel.Prerelease;
        var latestVersion = new SemanticVersion(2, 0, 0, "beta1");
        var toolPath = "tool-path";
        var message = "message";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s.Contains("A newer prerelease version of the ABP CLI is available: 2.0.0-beta1."))),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == message)),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == string.Empty)),
            Times.Exactly(2));
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == "Update Command: ")),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == "dotnet tool update --tool-path tool-path Volo.Abp.Cli --version 2.0.0-beta1")),
            Times.Once);
    }

    [Fact]
    public async Task LogNewVersionInfo_LogsWarning_WhenNewNightlyVersionIsAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CliService>>();
        var cliService = new CliService(
            Mock.Of<ICommandLineArgumentParser>(),
            Mock.Of<ICommandSelector>(),
            Mock.Of<IServiceScopeFactory>(),
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<MemoryService>(),
            Mock.Of<CliVersionService>(),
            Mock.Of<ITelemetryService>()
        );
        cliService.Logger = loggerMock.Object;

        var updateChannel = UpdateChannel.Nightly;
        var latestVersion = new SemanticVersion(2, 0, 0, "nightly");
        var toolPath = "tool-path";
        var message = "message";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s.Contains("A newer nightly version of the ABP CLI is available: 2.0.0-nightly."))),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == message)),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == string.Empty)),
            Times.Exactly(2));
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == "Update Command: ")),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == "dotnet tool uninstall --tool-path tool-path Volo.Abp.Cli")),
            Times.Once);
        loggerMock.Verify(
            x => x.LogWarning(It.Is<string>(s => s == "dotnet tool install --tool-path tool-path Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 2.0.0-nightly")),
            Times.Once);
    }
}
