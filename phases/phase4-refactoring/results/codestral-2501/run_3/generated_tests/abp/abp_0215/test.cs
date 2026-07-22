using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Events;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly Mock<ConnectionStringProvider> _connectionStringProviderMock;
        private readonly Mock<SolutionPackageVersionFinder> _solutionPackageVersionFinderMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<IInstallLibsService> _installLibsServiceMock;
        private readonly Mock<CliService> _cliServiceMock;
        private readonly Mock<AngularPwaSupportAdder> _angularPwaSupportAdderMock;
        private readonly Mock<InitialMigrationCreator> _initialMigrationCreatorMock;
        private readonly Mock<ThemePackageAdder> _themePackageAdderMock;
        private readonly Mock<ILocalEventBus> _eventBusMock;
        private readonly Mock<IBundlingService> _bundlingServiceMock;
        private readonly Mock<AngularThemeConfigurer> _angularThemeConfigurerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        private readonly TestProjectCreationCommand _commandBase;

        public ProjectCreationCommandBaseTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _connectionStringProviderMock = new Mock<ConnectionStringProvider>();
            _solutionPackageVersionFinderMock = new Mock<SolutionPackageVersionFinder>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _installLibsServiceMock = new Mock<IInstallLibsService>();
            _cliServiceMock = new Mock<CliService>();
            _angularPwaSupportAdderMock = new Mock<AngularPwaSupportAdder>();
            _initialMigrationCreatorMock = new Mock<InitialMigrationCreator>();
            _themePackageAdderMock = new Mock<ThemePackageAdder>();
            _eventBusMock = new Mock<ILocalEventBus>();
            _bundlingServiceMock = new Mock<IBundlingService>();
            _angularThemeConfigurerMock = new Mock<AngularThemeConfigurer>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _commandBase = new TestProjectCreationCommand(
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
                _cliVersionServiceMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionOptionIsProvided()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("--version 1.0.0");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Version: 1.0.0"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPreview_WhenPreviewOptionIsProvided()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("--preview");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Preview: yes"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPwa_WhenPwaOptionIsProvided()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("--pwa");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Progressive Web App: yes"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogDatabaseProvider_WhenDatabaseProviderOptionIsProvided()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("--database-provider SqlServer");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Database provider: SqlServer"),
                Times.Once
            );
        }
    }

    public class TestProjectCreationCommand : ProjectCreationCommandBase
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
            : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService, cliService, angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus, bundlingService, angularThemeConfigurer, cliVersionService)
        {
        }

        public new Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
        {
            return base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
        }
    }
}
