using Microsoft.Extensions.Logging;
using Moq;
using System;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public void LogNewVersionInfo_LogsWarning_WhenNewVersionIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CliService>>();
            var cliService = new CliService(
                Mock.Of<Volo.Abp.Cli.Args.ICommandLineArgumentParser>(),
                Mock.Of<Volo.Abp.Cli.Commands.ICommandSelector>(),
                Mock.Of<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>(),
                Mock.Of<Volo.Abp.Cli.Version.PackageVersionCheckerService>(),
                new Volo.Abp.Cli.CmdHelper(),
                Mock.Of<Volo.Abp.Cli.Memory.MemoryService>(),
                Mock.Of<Volo.Abp.Cli.Version.CliVersionService>(),
                Mock.Of<Volo.Abp.Internal.Telemetry.ITelemetryService>()
            );
            cliService.Logger = loggerMock.Object;

            var updateChannel = CliService.UpdateChannel.Stable;
            var latestVersion = new NuGet.Versioning.NuGetVersion(1, 2, 3);
            var toolPath = "toolPath";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 1.2.3."))),
                Times.Once);
        }
    }
}
