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
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsVersionInformation_WhenVersionOptionIsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var cliVersionServiceMock = new Mock<CliVersionService>(MockBehavior.Strict, null, null, null);
        cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync())
            .ReturnsAsync(new CliVersion { IsPrerelease = true });

        var commandLineArgs = new CommandLineArgs();
        commandLineArgs.Options["--version"] = "1.2.3";

        var sut = new TestProjectCreationCommand(
            connectionStringProvider: null,
            solutionPackageVersionFinder: null,
            cmdHelper: null,
            installLibsService: null,
            cliService: null,
            angularPwaSupportAdder: null,
            initialMigrationCreator: null,
            themePackageAdder: null,
            eventBus: null,
            bundlingService: null,
            angularThemeConfigurer: null,
            cliVersionService: cliVersionServiceMock.Object
        );
        sut.Logger = loggerMock.Object;

        // Act
        await sut.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

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
    public async Task GetProjectBuildArgsAsync_LogsPreviewInformation_WhenPreviewOptionIsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var cliVersionServiceMock = new Mock<CliVersionService>(MockBehavior.Strict, null, null, null);
        cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync())
            .ReturnsAsync(new CliVersion { IsPrerelease = true });

        var commandLineArgs = new CommandLineArgs();
        commandLineArgs.Options["--preview"] = "";

        var sut = new TestProjectCreationCommand(
            connectionStringProvider: null,
            solutionPackageVersionFinder: null,
            cmdHelper: null,
            installLibsService: null,
            cliService: null,
            angularPwaSupportAdder: null,
            initialMigrationCreator: null,
            themePackageAdder: null,
            eventBus: null,
            bundlingService: null,
            angularThemeConfigurer: null,
            cliVersionService: cliVersionServiceMock.Object
        );
        sut.Logger = loggerMock.Object;

        // Act
        await sut.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

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
}
