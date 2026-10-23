using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private class DummyCommand : ProjectCreationCommandBase
        {
            public DummyCommand(
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

            protected override Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_Version_When_Version_Is_Present()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = mockLogger.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Options = new OptionCollection
                {
                    { Options.Version.Short, "1.0.0" }
                }
            };

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Version: 1.0.0"))),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_Preview_When_Preview_Is_Present()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = mockLogger.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Options = new OptionCollection
                {
                    { Options.Preview.Long, "true" }
                }
            };

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Preview: yes"),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_Pwa_When_Pwa_Is_Present()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = mockLogger.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Options = new OptionCollection
                {
                    { Options.ProgressiveWebApp.Short, "true" }
                }
            };

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Progressive Web App: yes"),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_DatabaseProvider_When_Present()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = mockLogger.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Options = new OptionCollection
                {
                    { "db", "SqlServer" }
                }
            };

            // Mock GetDatabaseProvider to return a specific value
            var mockCommand = new Mock<DummyCommand>(
                null, null, null, null, null, null, null, null, null, null, null, null);
            mockCommand.CallBase = true;
            mockCommand.Setup(c => c.GetDatabaseProvider(It.IsAny<CommandLineArgs>()))
                .Returns(DatabaseProvider.SqlServer);

            // Act
            await mockCommand.Object.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            mockLogger.Verify(
                x => x.LogInformation($"Database provider: {DatabaseProvider.SqlServer}"),
                Times.Once);
        }
    }
}
