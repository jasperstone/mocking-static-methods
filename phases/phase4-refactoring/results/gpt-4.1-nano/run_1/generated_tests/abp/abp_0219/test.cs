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

            public Task<ProjectBuildArgs> DummyGetProjectBuildArgsAsync(CommandLineArgs args, string template, string projectName)
            {
                // For testing, just return a completed task
                return Task.FromResult<ProjectBuildArgs>(null);
            }
        }

        [Fact]
        public async Task LogInformation_Is_Called_For_Preview()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var mockCliVersionService = new Mock<CliVersionService>();
            mockCliVersionService.Setup(s => s.GetCurrentCliVersionAsync())
                .ReturnsAsync(new CliVersion { IsPrerelease = true });
            var command = new TestCommand(
                null, null, null, null, null, null, null, null, null, null, null, mockCliVersionService.Object);
            command.Logger = mockLogger.Object;

            var args = new CommandLineArgs
            {
                Options = new System.Collections.Generic.Dictionary<string, string>
                {
                    { Options.Preview.Long, "true" }
                }
            };

            // Act
            await command.GetProjectBuildArgsAsync(args, "template", "projectName");

            // Assert
            mockLogger.Verify(x => x.LogInformation("Preview: yes"), Times.Once);
        }
    }
}
