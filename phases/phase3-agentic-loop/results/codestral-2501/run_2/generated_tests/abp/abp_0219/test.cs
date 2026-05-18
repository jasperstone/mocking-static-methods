using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Microservice;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Cli.Commands.Services;
using Microsoft.Extensions.Options;

namespace Volo.Abp.Cli.Core.Tests.Commands
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
            _commandBase = new NewCommand(
                new ConnectionStringProvider(),
                new SolutionPackageVersionFinder(),
                new CmdHelper(new OptionsSnapshot<AbpCliOptions>()),
                new Mock<IInstallLibsService>().Object,
                new Mock<CliService>().Object,
                new Mock<AngularPwaSupportAdder>().Object,
                new Mock<InitialMigrationCreator>().Object,
                new Mock<ThemePackageAdder>().Object,
                new LocalEventBus(),
                new Mock<IBundlingService>().Object,
                new Mock<ITemplateInfoProvider>().Object,
                new Mock<TemplateProjectBuilder>().Object,
                new Mock<AngularThemeConfigurer>().Object,
                _cliVersionServiceMock.Object,
                new Mock<ITelemetryService>().Object
            );
            _commandBase.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogConnectionString()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "new", "TestProject" }, new Dictionary<string, string>
            {
                { Options.ConnectionString.Short, "TestConnectionString" }
            });

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Connection string: TestConnectionString"),
                Times.Once);
        }
    }
}
