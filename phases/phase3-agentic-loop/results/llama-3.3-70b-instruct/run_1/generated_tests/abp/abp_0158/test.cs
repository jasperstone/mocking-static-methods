using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public async Task LogNewVersionInfo_LogsWarning_WhenNewVersionIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                Mock.Of<Volo.Abp.Cli.Args.ICommandLineArgumentParser>(),
                Mock.Of<Volo.Abp.Cli.Commands.ICommandSelector>(),
                Mock.Of<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>(),
                Mock.Of<Volo.Abp.Cli.Version.PackageVersionCheckerService>(),
                Mock.Of<Volo.Abp.Cli.Utils.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Memory.MemoryService>(),
                Mock.Of<Volo.Abp.Cli.Version.CliVersionService>(),
                Mock.Of<Volo.Abp.Internal.Telemetry.ITelemetryService>()
            );
            cliService.Logger = loggerMock.Object;

            var updateChannel = CliService.UpdateChannel.Stable;
            var latestVersion = new NuGet.Versioning.SemanticVersion(2, 0, 0);
            var toolPath = "tool-path";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task LogNewVersionInfo_LogsWarningWithUpdateCommand_WhenNewVersionIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                Mock.Of<Volo.Abp.Cli.Args.ICommandLineArgumentParser>(),
                Mock.Of<Volo.Abp.Cli.Commands.ICommandSelector>(),
                Mock.Of<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>(),
                Mock.Of<Volo.Abp.Cli.Version.PackageVersionCheckerService>(),
                Mock.Of<Volo.Abp.Cli.Utils.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Memory.MemoryService>(),
                Mock.Of<Volo.Abp.Cli.Version.CliVersionService>(),
                Mock.Of<Volo.Abp.Internal.Telemetry.ITelemetryService>()
            );
            cliService.Logger = loggerMock.Object;

            var updateChannel = CliService.UpdateChannel.Stable;
            var latestVersion = new NuGet.Versioning.SemanticVersion(2, 0, 0);
            var toolPath = "tool-path";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    "dotnet tool update -g Volo.Abp.Cli",
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task LogNewVersionInfo_LogsWarningWithPrereleaseUpdateCommand_WhenNewPrereleaseVersionIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                Mock.Of<Volo.Abp.Cli.Args.ICommandLineArgumentParser>(),
                Mock.Of<Volo.Abp.Cli.Commands.ICommandSelector>(),
                Mock.Of<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>(),
                Mock.Of<Volo.Abp.Cli.Version.PackageVersionCheckerService>(),
                Mock.Of<Volo.Abp.Cli.Utils.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Memory.MemoryService>(),
                Mock.Of<Volo.Abp.Cli.Version.CliVersionService>(),
                Mock.Of<Volo.Abp.Internal.Telemetry.ITelemetryService>()
            );
            cliService.Logger = loggerMock.Object;

            var updateChannel = CliService.UpdateChannel.Prerelease;
            var latestVersion = new NuGet.Versioning.SemanticVersion(2, 0, 0, "beta1");
            var toolPath = "tool-path";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    "dotnet tool update -g Volo.Abp.Cli --version 2.0.0-beta1",
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task LogNewVersionInfo_LogsWarningWithNightlyUpdateCommand_WhenNewNightlyVersionIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                Mock.Of<Volo.Abp.Cli.Args.ICommandLineArgumentParser>(),
                Mock.Of<Volo.Abp.Cli.Commands.ICommandSelector>(),
                Mock.Of<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>(),
                Mock.Of<Volo.Abp.Cli.Version.PackageVersionCheckerService>(),
                Mock.Of<Volo.Abp.Cli.Utils.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.Memory.MemoryService>(),
                Mock.Of<Volo.Abp.Cli.Version.CliVersionService>(),
                Mock.Of<Volo.Abp.Internal.Telemetry.ITelemetryService>()
            );
            cliService.Logger = loggerMock.Object;

            var updateChannel = CliService.UpdateChannel.Nightly;
            var latestVersion = new NuGet.Versioning.SemanticVersion(2, 0, 0, "nightly");
            var toolPath = "tool-path";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    "dotnet tool uninstall -g Volo.Abp.Cli",
                    It.IsAny<object[]>()
                ),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(
                    "dotnet tool install -g Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 2.0.0-nightly",
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
