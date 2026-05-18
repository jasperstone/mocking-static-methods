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

        public new async Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
        {
            return await base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
        }
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsVersionInformation_WhenVersionOptionIsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var cliVersionServiceMock = new Mock<CliVersionService>(null, null, null, null, null, null, null, null, null, null, null);
        cliVersionServiceMock.Setup(s => s.GetCurrentCliVersionAsync())
            .ReturnsAsync(new CliVersion { IsPrerelease = true });

        var command = new TestProjectCreationCommand(
            null, null, null, null, null, null, null, null, null, null, null, cliVersionServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        var args = new CommandLineArgs();
        args.Options["v"] = "1.2.3"; // Version.Short is "v"

        // Act
        await command.GetProjectBuildArgsAsync(args, "template", "projectName");

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Version: 1.2.3")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
