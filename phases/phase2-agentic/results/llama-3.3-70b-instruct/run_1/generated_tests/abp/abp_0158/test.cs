using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli;
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
            var latestVersion = new SemanticVersion(2, 0, 0);
            var toolPath = "tool-path";
            var message = "message";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    $"A newer {updateChannel.ToString().ToLowerInvariant()} version of the ABP CLI is available: {latestVersion}."
                ),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(message),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(string.Empty),
                Times.Exactly(2)
            );

            loggerMock.Verify(
                x => x.LogWarning("Update Command: "),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning($"dotnet tool update {toolPath} Volo.Abp.Cli"),
                Times.Once
            );
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
                x => x.LogWarning(
                    $"A newer {updateChannel.ToString().ToLowerInvariant()} version of the ABP CLI is available: {latestVersion}."
                ),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(message),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(string.Empty),
                Times.Exactly(2)
            );

            loggerMock.Verify(
                x => x.LogWarning("Update Command: "),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning($"dotnet tool update {toolPath} Volo.Abp.Cli --version {latestVersion}"),
                Times.Once
            );
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
            var latestVersion = new SemanticVersion(2, 0, 0, "beta1");
            var toolPath = "tool-path";
            var message = "message";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    $"A newer {updateChannel.ToString().ToLowerInvariant()} version of the ABP CLI is available: {latestVersion}."
                ),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(message),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning(string.Empty),
                Times.Exactly(2)
            );

            loggerMock.Verify(
                x => x.LogWarning("Update Command: "),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning($"dotnet tool uninstall {toolPath} Volo.Abp.Cli"),
                Times.Once
            );

            loggerMock.Verify(
                x => x.LogWarning($"dotnet tool install {toolPath} Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version {latestVersion}"),
                Times.Once
            );
        }
    }
}
