using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Microservice;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.Cli.ProjectBuilding.Events;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly ProjectCreationCommandBase _commandBase;

        public ProjectCreationCommandBaseTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _commandBase = new ProjectCreationCommandBase(
                new ConnectionStringProvider(),
                new SolutionPackageVersionFinder(),
                new CmdHelper(),
                new InstallLibsService(),
                new CliService(),
                new AngularPwaSupportAdder(),
                new InitialMigrationCreator(),
                new ThemePackageAdder(),
                new LocalEventBus(),
                new BundlingService(),
                new AngularThemeConfigurer(),
                _cliVersionServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogConnectionString()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "new", "MyProject" }, new Dictionary<string, string>
            {
                { Options.ConnectionString.Long, "TestConnectionString" }
            });
            var template = "app";
            var projectName = "MyProject";

            // Act
            var result = await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Connection string: TestConnectionString"), Times.Once);
        }
    }
}
