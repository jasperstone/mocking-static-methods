using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Microservice;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;

public class ProjectCreationCommandBaseTests
{
    private readonly Mock<ILogger<ProjectCreationCommandBase>> _loggerMock;
    private readonly Mock<ICliVersionService> _cliVersionServiceMock;
    private readonly Mock<ICmdHelper> _cmdHelperMock;
    private readonly Mock<IInstallLibsService> _installLibsServiceMock;
    private readonly Mock<CliService> _cliServiceMock;
    private readonly Mock<AngularPwaSupportAdder> _angularPwaSupportAdderMock;
    private readonly Mock<InitialMigrationCreator> _initialMigrationCreatorMock;
    private readonly Mock<ILocalEventBus> _eventBusMock;
    private readonly Mock<ThemePackageAdder> _themePackageAdderMock;
    private readonly Mock<AngularThemeConfigurer> _angularThemeConfigurerMock;
    private readonly Mock<IBundlingService> _bundlingServiceMock;
    private readonly Mock<ConnectionStringProvider> _connectionStringProviderMock;
    private readonly Mock<SolutionPackageVersionFinder> _solutionPackageVersionFinderMock;

    public ProjectCreationCommandBaseTests()
    {
        _loggerMock = new Mock<ILogger<ProjectCreationCommandBase>>();
        _cliVersionServiceMock = new Mock<ICliVersionService>();
        _cmdHelperMock = new Mock<ICmdHelper>();
        _installLibsServiceMock = new Mock<IInstallLibsService>();
        _cliServiceMock = new Mock<CliService>();
        _angularPwaSupportAdderMock = new Mock<AngularPwaSupportAdder>();
        _initialMigrationCreatorMock = new Mock<InitialMigrationCreator>();
        _eventBusMock = new Mock<ILocalEventBus>();
        _themePackageAdderMock = new Mock<ThemePackageAdder>();
        _angularThemeConfigurerMock = new Mock<AngularThemeConfigurer>();
        _bundlingServiceMock = new Mock<IBundlingService>();
        _connectionStringProviderMock = new Mock<ConnectionStringProvider>();
        _solutionPackageVersionFinderMock = new Mock<SolutionPackageVersionFinder>();
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsVersion_WhenVersionIsSpecified()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs
        {
            Options = new CommandLineOptions
            {
                { Options.Version.Short, "1.0.0" }
            }
        };
        var template = "template";
        var projectName = "projectName";

        var command = new MockProjectCreationCommandBase(
            _connectionStringProviderMock.Object,
            _solutionPackageVersionFinderMock.Object,
            _cmdHelperMock.Object,
            _installLibsServiceMock.Object,
            _cliServiceMock.Object,
            _angularPwaSupportAdderMock.Object,
            _initialMigrationCreatorMock.Object,
            _themePackageAdderMock.Object,
            _eventBusMock.Object,
            _bundlingServiceMock.Object,
            _angularThemeConfigurerMock.Object,
            _cliVersionServiceMock.Object)
        {
            Logger = _loggerMock.Object
        };

        // Act
        await command.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogInformation(It.Is<string>(s => s.Contains("Version: 1.0.0"))),
            Times.Once);
    }

    private class MockProjectCreationCommandBase : ProjectCreationCommandBase
    {
        public MockProjectCreationCommandBase(
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
            : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService, cliService, angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus, bundlingService, angularThemeConfigurer, cliVersionService)
        {
        }
    }
}
