using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.LIbs;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Events;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.ProjectModification;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly TestProjectCreationCommandBase _commandBase;

        public ProjectCreationCommandBaseTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _commandBase = new TestProjectCreationCommandBase(
                Mock.Of<ConnectionStringProvider>(),
                Mock.Of<SolutionPackageVersionFinder>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<IInstallLibsService>(),
                new CliService(), // Use the real implementation
                Mock.Of<AngularPwaSupportAdder>(),
                Mock.Of<InitialMigrationCreator>(),
                Mock.Of<ThemePackageAdder>(),
                Mock.Of<ILocalEventBus>(),
                Mock.Of<IBundlingService>(),
                Mock.Of<ITemplateInfoProvider>(),
                Mock.Of<TemplateProjectBuilder>(),
                Mock.Of<AngularThemeConfigurer>(),
                Mock.Of<CliVersionService>(),
                Mock.Of<ITelemetryService>()
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("new projectName -v 1.0.0");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Version: 1.0.0")),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPreview_WhenPreviewIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("new projectName --preview");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Preview: yes")),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogPwa_WhenPwaIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("new projectName -pwa");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Progressive Web App: yes")),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_ShouldLogDatabaseProvider_WhenDatabaseProviderIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs("new projectName -db SqlServer");

            // Act
            await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Database provider: SqlServer")),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestProjectCreationCommandBase : ProjectCreationCommandBase
        {
            public TestProjectCreationCommandBase(
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
                ITemplateInfoProvider templateInfoProvider,
                TemplateProjectBuilder templateProjectBuilder,
                AngularThemeConfigurer angularThemeConfigurer,
                CliVersionService cliVersionService,
                ITelemetryService telemetryService) :
                base(connectionStringProvider,
                    solutionPackageVersionFinder,
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
            }

            public new Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                return base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
            }
        }
    }
}
