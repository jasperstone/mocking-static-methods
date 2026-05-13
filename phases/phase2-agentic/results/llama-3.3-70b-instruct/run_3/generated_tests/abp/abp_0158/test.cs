using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Core;
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
            var latestVersion = new SemanticVersion("2.0.0");
            var toolPath = "tool-path";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

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
            var latestVersion = new SemanticVersion("2.0.0-beta");
            var toolPath = "tool-path";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("A newer prerelease version of the ABP CLI is available: 2.0.0-beta."))),
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
            var latestVersion = new SemanticVersion("2.0.0-nightly");
            var toolPath = "tool-path";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("A newer nightly version of the ABP CLI is available: 2.0.0-nightly."))),
                Times.Once);
        }
    }
}
