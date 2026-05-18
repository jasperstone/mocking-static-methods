using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Core;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
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

            var updateChannel = Volo.Abp.Cli.Core.CliService.UpdateChannel.Stable;
            var latestVersion = new SemanticVersion(2, 0, 0);
            var toolPath = "toolPath";
            var message = "message";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 2.0.0."))),
                Times.Once);
        }

        [Fact]
        public async Task LogNewVersionInfo_LogsWarning_WhenNewPrereleaseVersionIsAvailable()
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

            var updateChannel = Volo.Abp.Cli.Core.CliService.UpdateChannel.Prerelease;
            var latestVersion = new SemanticVersion(2, 0, 0, "beta1");
            var toolPath = "toolPath";
            var message = "message";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("A newer prerelease version of the ABP CLI is available: 2.0.0-beta1."))),
                Times.Once);
        }

        [Fact]
        public async Task LogNewVersionInfo_LogsWarning_WhenNewNightlyVersionIsAvailable()
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

            var updateChannel = Volo.Abp.Cli.Core.CliService.UpdateChannel.Nightly;
            var latestVersion = new SemanticVersion(2, 0, 0);
            var toolPath = "toolPath";
            var message = "message";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("A newer nightly version of the ABP CLI is available: 2.0.0."))),
                Times.Once);
        }
    }
}
