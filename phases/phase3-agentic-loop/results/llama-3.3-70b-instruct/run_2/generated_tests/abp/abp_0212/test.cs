using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsTieredInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new NewCommand(
                Mock.Of<Volo.Abp.Cli.Args.ConnectionStringProvider>(),
                Mock.Of<Volo.Abp.Cli.Version.SolutionPackageVersionFinder>(),
                Mock.Of<Volo.Abp.Cli.Utils.ICmdHelper>(),
                Mock.Of<Volo.Abp.Cli.LIbs.IInstallLibsService>(),
                Mock.Of<Volo.Abp.Cli.CliService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.AngularPwaSupportAdder>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.InitialMigrationCreator>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.ThemePackageAdder>(),
                Mock.Of<Volo.Abp.EventBus.Local.ILocalEventBus>(),
                Mock.Of<Volo.Abp.Cli.Bundling.IBundlingService>(),
                Mock.Of<Volo.Abp.Cli.ProjectModification.AngularThemeConfigurer>(),
                Mock.Of<Volo.Abp.Cli.Version.CliVersionService>(),
                Mock.Of<Volo.Abp.Internal.Telemetry.ITelemetryService>()
            );
            command.Logger = loggerMock.Object;
            var commandLineArgs = new Volo.Abp.Cli.Args.CommandLineArgs
            {
                Target = "MyProject",
                Options = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "--tiered", string.Empty }
                }
            };

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Tiered: yes"), Times.Once);
        }
    }
}
