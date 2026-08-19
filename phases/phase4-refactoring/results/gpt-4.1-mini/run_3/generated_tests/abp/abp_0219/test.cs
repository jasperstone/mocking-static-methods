using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class ProjectCreationCommandBaseTests
{
    private class TestProjectCreationCommand : ProjectCreationCommandBase
    {
        public TestProjectCreationCommand()
            : base(
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
                cliVersionService: null)
        {
        }

        public new async Task GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
        {
            await base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
        }
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsConnectionString_WhenOptionIsSet()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var command = new TestProjectCreationCommand();
        command.Logger = loggerMock.Object;

        var args = new CommandLineArgs();
        args.Options["ConnectionString"] = "Server=myServer;Database=myDb;";

        // Act
        await command.GetProjectBuildArgsAsync(args, "template", "projectName");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Connection string: Server=myServer;Database=myDb;")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
            Times.Once);
    }
}
