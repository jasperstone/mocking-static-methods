using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

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

        // Expose protected method for testing
        public new Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
        {
            return base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
        }

        // Override methods that are called inside GetProjectBuildArgsAsync to avoid null refs
        protected override DatabaseProvider GetDatabaseProvider(CommandLineArgs commandLineArgs) => DatabaseProvider.NotSpecified;
        protected override string GetConnectionString(CommandLineArgs commandLineArgs) => null;
        protected override DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) => DatabaseManagementSystem.NotSpecified;
        protected override UiFramework GetUiFramework(CommandLineArgs commandLineArgs, string template) => UiFramework.NotSpecified;
        protected override bool GetCreateSolutionFolderPreference(CommandLineArgs commandLineArgs) => false;
        protected override MobileApp GetMobilePreference(CommandLineArgs commandLineArgs, string template) => MobileApp.None;
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsVersionInformation_WhenVersionOptionIsSet()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var cliVersionServiceMock = new Mock<CliVersionService>(null, null);
        var commandLineArgs = new CommandLineArgs();
        commandLineArgs.Options["--version"] = "1.2.3";

        var command = new TestProjectCreationCommand(
            null, null, null, null, null, null, null, null, null, null, null, cliVersionServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Version: 1.2.3")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsPreviewInformation_WhenPreviewOptionIsSet()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var cliVersionServiceMock = new Mock<CliVersionService>(null, null);
        var commandLineArgs = new CommandLineArgs();
        commandLineArgs.Options["--preview"] = "";

        var command = new TestProjectCreationCommand(
            null, null, null, null, null, null, null, null, null, null, null, cliVersionServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Setup CliVersionService to return a prerelease version to avoid exception
        cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync())
            .ReturnsAsync(new CliVersion { IsPrerelease = true });

        // Act
        await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Preview: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsProgressiveWebAppInformation_WhenPwaOptionIsSet()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var cliVersionServiceMock = new Mock<CliVersionService>(null, null);
        var commandLineArgs = new CommandLineArgs();
        commandLineArgs.Options["-pwa"] = "";

        var command = new TestProjectCreationCommand(
            null, null, null, null, null, null, null, null, null, null, null, cliVersionServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Progressive Web App: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
