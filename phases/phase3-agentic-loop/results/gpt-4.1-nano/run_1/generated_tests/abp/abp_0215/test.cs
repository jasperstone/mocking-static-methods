using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

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
                : base(
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
                    cliVersionService)
            {
            }

            public Task<ProjectBuildArgs> DummyGetProjectBuildArgsAsync(CommandLineArgs args, string template, string projectName)
            {
                // For testing, just return null or a dummy object
                return Task.FromResult<ProjectBuildArgs>(null);
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_Version_When_Version_Is_Present()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new DummyCommand(
                null, null, null, null, null, null, null, null, null, null, null, null);
            command.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Options = new OptionCollection
                {
                    { Options.Version.Short, "1.0.0" }
                }
            };

            // Act
            await command.GetType().GetMethod("GetProjectBuildArgsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(command, new object[] { commandLineArgs, "template", "projectName" });

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Version: 1.0.0"),
                Times.Once);
        }
    }
}
