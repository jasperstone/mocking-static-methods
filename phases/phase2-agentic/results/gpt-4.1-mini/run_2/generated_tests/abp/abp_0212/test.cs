using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationIncludingTieredYes_WhenTieredOptionIsPresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>(
                MockBehavior.Strict,
                null, null, null, null, null, null, null, null, null, null, null, null);

            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            templateInfoProviderMock.Setup(t => t.GetDefaultAsync())
                .ReturnsAsync(new TemplateInfo { Name = "app" });

            var newCommand = new NewCommand(
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
                templateInfoProvider: templateInfoProviderMock.Object,
                templateProjectBuilder: templateProjectBuilderMock.Object,
                angularThemeConfigurer: null,
                cliVersionService: null,
                telemetryService: telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var options = new AbpCommandLineOptions();
            options["tiered"] = ""; // simulate presence of --tiered option
            var commandLineArgs = new CommandLineArgs("new", "MyProject")
            {
                Options = options
            };

            var projectBuildArgs = new ProjectBuildArgs
            {
                OutputFolder = "output"
            };

            // Setup TemplateProjectBuilder.BuildAsync to return a dummy result
            templateProjectBuilderMock.Setup(t => t.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult());

            // Setup GetProjectBuildArgsAsync to return our projectBuildArgs
            var getProjectBuildArgsAsyncMethod = typeof(NewCommand).GetMethod("GetProjectBuildArgsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<ProjectBuildArgs>)getProjectBuildArgsAsyncMethod.Invoke(newCommand, new object[] { commandLineArgs, "app", "MyProject" });
            // We cannot invoke private method directly, so we will mock it by subclassing NewCommand

            // Instead, create a subclass to override GetProjectBuildArgsAsync
            var newCommandWithOverride = new NewCommandWithOverrides(
                templateInfoProviderMock.Object,
                templateProjectBuilderMock.Object,
                telemetryServiceMock.Object,
                loggerMock.Object,
                projectBuildArgs);

            // Act
            await newCommandWithOverride.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating your project...")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Project name: MyProject")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tiered: yes")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'MyProject' has been successfully created to 'output'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        private class NewCommandWithOverrides : NewCommand
        {
            private readonly ProjectBuildArgs _projectBuildArgs;
            private readonly ILogger _logger;

            public NewCommandWithOverrides(
                ITemplateInfoProvider templateInfoProvider,
                TemplateProjectBuilder templateProjectBuilder,
                ITelemetryService telemetryService,
                ILogger logger,
                ProjectBuildArgs projectBuildArgs)
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
                    templateInfoProvider: templateInfoProvider,
                    templateProjectBuilder: templateProjectBuilder,
                    angularThemeConfigurer: null,
                    cliVersionService: null,
                    telemetryService: telemetryService)
            {
                _projectBuildArgs = projectBuildArgs;
                Logger = logger;
                _logger = logger;
            }

            public override ILogger Logger { get; set; }

            protected override Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                return Task.FromResult(_projectBuildArgs);
            }

            protected override Task CheckCreatingRequirements(ProjectBuildArgs projectArgs)
            {
                return Task.CompletedTask;
            }

            protected override Task CheckCreatedRequirements(ProjectBuildArgs projectArgs)
            {
                return Task.CompletedTask;
            }

            protected override Task CreateOpenIddictPfxFilesAsync(ProjectBuildArgs projectArgs)
            {
                return Task.CompletedTask;
            }

            protected override Task RunGraphBuildForMicroserviceServiceTemplate(ProjectBuildArgs projectArgs)
            {
                return Task.CompletedTask;
            }

            protected override Task CreateInitialMigrationsAsync(ProjectBuildArgs projectArgs)
            {
                return Task.CompletedTask;
            }

            protected override Task ConfigureAngularAfterMicroserviceServiceCreatedAsync(ProjectBuildArgs projectArgs, string template)
            {
                return Task.CompletedTask;
            }

            protected override Task RunInstallLibsForWebTemplateAsync(ProjectBuildArgs projectArgs)
            {
                return Task.CompletedTask;
            }

            protected override void ConfigureAngularJsonForThemeSelection(ProjectBuildArgs projectArgs)
            {
            }

            protected override Task RunBundleInternalAsync(ProjectBuildArgs projectArgs)
            {
                return Task.CompletedTask;
            }

            protected override Task ConfigurePwaSupportForAngular(ProjectBuildArgs projectArgs)
            {
                return Task.CompletedTask;
            }

            protected override void OpenRelatedWebPage(ProjectBuildArgs projectArgs, string template, bool isTiered, CommandLineArgs commandLineArgs)
            {
            }

            protected override void ExtractProjectZip(ProjectBuildResult result, string outputFolder)
            {
            }
        }
    }
}
