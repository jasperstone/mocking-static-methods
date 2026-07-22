using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private class TestCommand : ProjectCreationCommandBase
        {
            public TestCommand(
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

            public Task<ProjectBuildArgs> DummyGetProjectBuildArgsAsync() => Task.FromResult<ProjectBuildArgs>(null);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_Should_Log_Version_When_Version_Is_Present()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new TestCommand(
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
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Version: 1.0.0"))),
                Times.Once);
        }
    }
}
