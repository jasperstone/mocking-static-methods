using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
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
            commandLineArgs.Options.Add(Options.Version.Short, "1.0.0");
            var projectCreationCommandBase = new TestProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Version: 1.0.0"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPreview_WhenPreviewIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.Preview.Long, "");
            var projectCreationCommandBase = new TestProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Preview: yes"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsProgressiveWebApp_WhenProgressiveWebAppIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.ProgressiveWebApp.Short, "");
            var projectCreationCommandBase = new TestProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Progressive Web App: yes"), Times.Once);
        }
    }

    public class TestProjectCreationCommandBase : ProjectCreationCommandBase
    {
        public TestProjectCreationCommandBase(
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
            CliVersionService cliVersionService,
            ILogger<NewCommand> logger) 
            : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService, cliService, 
                  angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus, bundlingService, 
                  angularThemeConfigurer, cliVersionService)
        {
            Logger = logger;
        }
    }
}
