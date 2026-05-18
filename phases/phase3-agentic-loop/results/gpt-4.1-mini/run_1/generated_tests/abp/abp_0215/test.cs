using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        private class TestProjectCreationCommand : ProjectCreationCommandBase
        {
            public TestProjectCreationCommand(
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

            public new Task<object> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                // We return object because ProjectBuildArgs type is not accessible
                return base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName) as Task<object>;
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsVersionInformation_WhenVersionOptionIsPresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var cliVersionServiceMock = new Mock<CliVersionService>(null, null);
            cliVersionServiceMock.Setup(c => c.GetCurrentCliVersionAsync())
                .ReturnsAsync(new CliVersion { IsPrerelease = true });

            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options["--version"] = "1.2.3";

            var sut = new TestProjectCreationCommand(
                null, null, null, null, null, null, null, null, Mock.Of<ILocalEventBus>(), null, null, cliVersionServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await sut.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Version: 1.2.3")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
