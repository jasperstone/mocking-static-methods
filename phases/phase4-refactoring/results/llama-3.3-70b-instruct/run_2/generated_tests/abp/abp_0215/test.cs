using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Options;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.Version.Short, "1.0.0");
            var projectCreationCommandBase = new TestProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Version: 1.0.0"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPreview_WhenPreviewIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.Preview.Long, true);
            var projectCreationCommandBase = new TestProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Preview: yes"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPwa_WhenPwaIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.ProgressiveWebApp.Short, true);
            var projectCreationCommandBase = new TestProjectCreationCommandBase(
                null, null, null, null, null, null, null, null, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "", "");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Progressive Web App: yes"), Times.Once);
        }

        private class TestProjectCreationCommandBase : ProjectCreationCommandBase
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
                CliVersionService cliVersionService)
                : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService, cliService,
                      angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus, bundlingService,
                      angularThemeConfigurer, cliVersionService)
            {
            }
        }
    }
}
