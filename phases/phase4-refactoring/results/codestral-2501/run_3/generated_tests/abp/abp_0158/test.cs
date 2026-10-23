using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Memory;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using NuGet.Versioning;

public class CliServiceTests
{
    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectMessage()
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
        )
        {
            Logger = loggerMock.Object
        };

        var updateChannel = UpdateChannel.Stable;
        var latestVersion = new SemanticVersion(1, 0, 0);
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 1.0.0."))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli"))),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsCorrectMessage()
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
        )
        {
            Logger = loggerMock.Object
        };

        var updateChannel = UpdateChannel.Prerelease;
        var latestVersion = new SemanticVersion(1, 0, 0, "alpha");
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("A newer prerelease version of the ABP CLI is available: 1.0.0-alpha."))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool update --tool-path C:\\path\\to\\tool Volo.Abp.Cli --version 1.0.0-alpha"))),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_LogsCorrectMessage()
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
        )
        {
            Logger = loggerMock.Object
        };

        var updateChannel = UpdateChannel.Nightly;
        var latestVersion = new SemanticVersion(1, 0, 0, "nightly");
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("A newer nightly version of the ABP CLI is available: 1.0.0-nightly."))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli"))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 1.0.0-nightly"))),
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_DevelopmentChannel_LogsCorrectMessage()
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
        )
        {
            Logger = loggerMock.Object
        };

        var updateChannel = UpdateChannel.Development;
        var latestVersion = new SemanticVersion(1, 0, 0, "dev");
        var toolPath = @"C:\path\to\tool";

        // Act
        cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("A newer development version of the ABP CLI is available: 1.0.0-dev."))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool uninstall --tool-path C:\\path\\to\\tool Volo.Abp.Cli"))),
            Times.Once);

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("dotnet tool install --tool-path C:\\path\\to\\tool Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 1.0.0-dev"))),
            Times.Once);
    }
}
