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

        // We cannot override non-virtual methods, so we will simulate the behavior by setting options directly
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsConnectionString()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var command = new TestProjectCreationCommand();
        command.Logger = loggerMock.Object;

        var args = new CommandLineArgs();
        // Set the option that will cause the connection string to be logged
        args.Options["connectionString"] = "TestConnectionString";

        // Act
        await command.GetProjectBuildArgsAsync(args, "template", "projectName");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Connection string: TestConnectionString")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
            Times.AtLeastOnce);
    }
}
