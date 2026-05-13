using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Microservice;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly Mock<IBundlingService> _bundlingServiceMock;
        private readonly Mock<ConnectionStringProvider> _connectionStringProviderMock;
        private readonly Mock<SolutionPackageVersionFinder> _solutionPackageVersionFinderMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<IInstallLibsService> _installLibsServiceMock;
        private readonly Mock<CliService> _cliServiceMock;
        private readonly Mock<AngularPwaSupportAdder> _angularPwaSupportAdderMock;
        private readonly Mock<InitialMigrationCreator> _initialMigrationCreatorMock;
        private readonly Mock<ThemePackageAdder> _themePackageAdderMock;
        private readonly Mock<ILocalEventBus> _eventBusMock;
        private readonly Mock<AngularThemeConfigurer> _angularThemeConfigurerMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;

        private readonly ProjectCreationCommandBase _commandBase;

        public ProjectCreationCommandBaseTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _bundlingServiceMock = new Mock<IBundlingService>();
            _connectionStringProviderMock = new Mock<ConnectionStringProvider>();
            _solutionPackageVersionFinderMock = new Mock<SolutionPackageVersionFinder>();
            _cmdHelperMock = new Mock<ICmdHelper>();
            _installLibsServiceMock = new Mock<IInstallLibsService>();
            _cliServiceMock = new Mock<CliService>();
            _angularPwaSupportAdderMock = new Mock<AngularPwaSupportAdder>();
            _initialMigrationCreatorMock = new Mock<InitialMigrationCreator>();
            _themePackageAdderMock = new Mock<ThemePackageAdder>();
            _eventBusMock = new Mock<ILocalEventBus>();
            _angularThemeConfigurerMock = new Mock<AngularThemeConfigurer>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            _commandBase = new NewCommand(
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
                new Mock<ITemplateInfoProvider>().Object,
                new Mock<TemplateProjectBuilder>().Object,
                _angularThemeConfigurerMock.Object,
                _cliVersionServiceMock.Object,
                new Mock<ITelemetryService>().Object
            );
            _commandBase.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.Version.Short, "1.0.0");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Version: 1.0.0"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPreview_WhenPreviewIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.Preview.Long, "true");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Preview: yes"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPwa_WhenPwaIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.ProgressiveWebApp.Short, "true");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Progressive Web App: yes"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogDatabaseProvider_WhenDatabaseProviderIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add(Options.DatabaseProvider.Short, "sqlserver");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Database provider: sqlserver"),
                Times.Once
            );
        }
    }
}
