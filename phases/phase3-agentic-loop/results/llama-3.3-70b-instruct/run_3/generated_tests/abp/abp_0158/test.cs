using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Core.Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceTests
    {
        [Fact]
        public void LogNewVersionInfo_Stable_UpdateChannel_Logs_Update_Command()
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
            var latestVersion = new NuGet.Versioning.SemanticVersion(1, 2, 3);
            var toolPath = @"%USERPROFILE%\.dotnet\tools\Volo.Abp.Cli";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(message => message.Contains("dotnet tool update -g Volo.Abp.Cli"))),
                Times.Once);
        }

        [Fact]
        public void LogNewVersionInfo_Prerelease_UpdateChannel_Logs_Update_Command()
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
            var latestVersion = new NuGet.Versioning.SemanticVersion(1, 2, 3);
            var toolPath = @"%USERPROFILE%\.dotnet\tools\Volo.Abp.Cli";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(message => message.Contains("dotnet tool update -g Volo.Abp.Cli --version 1.2.3"))),
                Times.Once);
        }

        [Fact]
        public void LogNewVersionInfo_Nightly_UpdateChannel_Logs_Update_Command()
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
            var latestVersion = new NuGet.Versioning.SemanticVersion(1, 2, 3);
            var toolPath = @"%USERPROFILE%\.dotnet\tools\Volo.Abp.Cli";

            // Act
            cliService.LogNewVersionInfo(updateChannel, latestVersion, toolPath);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(message => message.Contains("dotnet tool uninstall -g Volo.Abp.Cli"))),
                Times.Once);

            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(message => message.Contains("dotnet tool install -g Volo.Abp.Cli --add-source https://www.myget.org/F/abp-nightly/api/v3/index.json --version 1.2.3"))),
                Times.Once);
        }
    }
}
