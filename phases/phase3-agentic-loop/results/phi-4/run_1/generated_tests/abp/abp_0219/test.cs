using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Volo.Abp.Cli.ProjectBuilding.Events;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Microservice;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;

namespace Volo.Abp.Cli.Tests.Commands
{
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
        private readonly Mock<IBundlingService> _bundlingServiceMock;
        private readonly Mock<ThemePackageAdder> _themePackageAdderMock;
        private readonly Mock<AngularThemeConfigurer> _angularThemeConfigurerMock;

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
            _bundlingServiceMock = new Mock<IBundlingService>();
            _themePackageAdderMock = new Mock<ThemePackageAdder>();
            _angularThemeConfigurerMock = new Mock<AngularThemeConfigurer>();
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString_WhenConnectionStringIsNotNull()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>
                {
                    { "ConnectionString", "TestConnectionString" }
                }
            };
            var template = "TestTemplate";
            var projectName = "TestProject";

            var projectCreationCommandBase = new MockProjectCreationCommandBase(
                _loggerMock.Object,
                _cliVersionServiceMock.Object,
                _cmdHelperMock.Object,
                _installLibsServiceMock.Object,
                _cliServiceMock.Object,
                _angularPwaSupportAdderMock.Object,
                _initialMigrationCreatorMock.Object,
                _themePackageAdderMock.Object,
                _eventBusMock.Object,
                _bundlingServiceMock.Object,
                _angularThemeConfigurerMock.Object);

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(It.Is<string>(s => s.Contains("Connection string: TestConnectionString"))),
                Times.Once);
        }

        private class MockProjectCreationCommandBase : ProjectCreationCommandBase
        {
            public MockProjectCreationCommandBase(
                ILogger<ProjectCreationCommandBase> logger,
                ICliVersionService cliVersionService,
                ICmdHelper cmdHelper,
                IInstallLibsService installLibsService,
                CliService cliService,
                AngularPwaSupportAdder angularPwaSupportAdder,
                InitialMigrationCreator initialMigrationCreator,
                ThemePackageAdder themePackageAdder,
                ILocalEventBus eventBus,
                IBundlingService bundlingService,
                AngularThemeConfigurer angularThemeConfigurer)
                : base(
                    new ConnectionStringProvider(),
                    new SolutionPackageVersionFinder(),
                    cmdHelper,
                    installLibsService,
                    cliService,
                    angularPwaSupportAdder,
                    initialMigrationCreator,
                    themePackageAdder,
                    eventBus,
                    bundlingService,
                    angularThemeConfigurer,
                    cliVersionService)
            {
                Logger = logger;
            }

            protected override string GetConnectionString(CommandLineArgs commandLineArgs)
            {
                return commandLineArgs.Options.GetOrNull("ConnectionString");
            }
        }
    }
}
