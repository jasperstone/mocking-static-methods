using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public void LogNewVersionInfo_LogsWarningMessage()
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
            var latestVersion = new NuGet.Versioning.NuGetVersion("1.2.3");
            var toolPath = "tool-path";
            var message = "message";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath, message);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    $"A newer {updateChannel.ToString().ToLowerInvariant()} version of the ABP CLI is available: {latestVersion}."),
                Times.Once);
            loggerMock.Verify(
                x => x.LogWarning(message),
                Times.Once);
            loggerMock.Verify(
                x => x.LogWarning(string.Empty),
                Times.Once);
            loggerMock.Verify(
                x => x.LogWarning("Update Command: "),
                Times.Once);
            loggerMock.Verify(
                x => x.LogWarning($"dotnet tool update {toolPath} Volo.Abp.Cli"),
                Times.Once);
            loggerMock.Verify(
                x => x.LogWarning(string.Empty),
                Times.Once);
        }
    }
}
