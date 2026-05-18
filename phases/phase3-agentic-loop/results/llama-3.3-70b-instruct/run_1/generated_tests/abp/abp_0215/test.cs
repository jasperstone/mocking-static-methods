using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.Cli.ProjectBuilding.Events;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Microservice;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options["--version"] = "1.0.0";
            var projectCreationCommandBase = new ProjectCreationCommandBaseTest(
                loggerMock.Object,
                new ConnectionStringProvider(),
                new SolutionPackageVersionFinder(),
                new CmdHelper(),
                new InstallLibsService(),
                new CliService(),
                new AngularPwaSupportAdder(),
                new InitialMigrationCreator(),
                new ThemePackageAdder(),
                new LocalEventBus(),
                new BundlingService(),
                new AngularThemeConfigurer(),
                new CliVersionService()
            );

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Version: 1.0.0"), Times.Once);
        }

        private class ProjectCreationCommandBaseTest : ProjectCreationCommandBase
        {
            public ProjectCreationCommandBaseTest(
                ILogger<NewCommand> logger,
                ConnectionStringProvider connectionStringProvider,
                SolutionPackageVersionFinder solutionPackageVersionFinder,
                ICmdHelper cmdHelper,
                IInstallLibsService installLibsService,
                CliService cliService,
                AngularPwaSupportAdder angularPwaSupportAdder,
                InitialMigrationCreator initialMigrationCreator,
                ThemePackageAdder themePackageAdder,
                ILocalEventBus eventBus,
                IBundlingService bundlingService,
                AngularThemeConfigurer angularThemeConfigurer,
                CliVersionService cliVersionService
            ) : base(
                connectionStringProvider,
                solutionPackageVersionFinder,
                cmdHelper,
                installLibsService,
                cliService,
                angularPwaSupportAdder,
                initialMigrationCreator,
                themePackageAdder,
                eventBus,
                bundlingService,
                angularThemeConfigurer,
                cliVersionService
            )
            {
                Logger = logger;
            }
        }
    }
}
