using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
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
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Internal.Telemetry.Constants.Enums;
using Microsoft.Extensions.Options;

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
            var optionsSnapshotMock = new Mock<IOptionsSnapshot<AbpCliOptions>>();
            var commandLineArgumentParserMock = new Mock<ICommandLineArgumentParser>();
            var commandSelectorMock = new Mock<ICommandSelector>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var memoryServiceMock = new Mock<MemoryService>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            _commandBase = new NewCommand(
                new ConnectionStringProvider(),
                new SolutionPackageVersionFinder(),
                new CmdHelper(optionsSnapshotMock.Object),
                new InstallLibsService(),
                new CliService(commandLineArgumentParserMock.Object, commandSelectorMock.Object, serviceScopeFactoryMock.Object, packageVersionCheckerServiceMock.Object, cmdHelperMock.Object, memoryServiceMock.Object, _cliVersionServiceMock.Object, telemetryServiceMock.Object),
                new AngularPwaSupportAdder(),
                new InitialMigrationCreator(),
                new ThemePackageAdder(),
                new LocalEventBus(),
                new BundlingService(),
                new TemplateInfoProvider(),
                new TemplateProjectBuilder(),
                new AngularThemeConfigurer(),
                _cliVersionServiceMock.Object,
                telemetryServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString()
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
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Connection string: TestConnectionString")),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
