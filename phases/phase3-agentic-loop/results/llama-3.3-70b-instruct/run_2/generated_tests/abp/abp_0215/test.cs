using Xunit;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsVersion_WhenVersionIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var connectionStringProviderMock = new Mock<ConnectionStringProvider>();
            var solutionPackageVersionFinderMock = new Mock<SolutionPackageVersionFinder>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var installLibsServiceMock = new Mock<IInstallLibsService>();
            var cliServiceMock = new Mock<CliService>();
            var angularPwaSupportAdderMock = new Mock<AngularPwaSupportAdder>();
            var initialMigrationCreatorMock = new Mock<InitialMigrationCreator>();
            var themePackageAdderMock = new Mock<ThemePackageAdder>();
            var eventBusMock = new Mock<ILocalEventBus>();
            var bundlingServiceMock = new Mock<IBundlingService>();
            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            var angularThemeConfigurerMock = new Mock<AngularThemeConfigurer>();
            var cliVersionServiceMock = new Mock<CliVersionService>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            var newCommand = new NewCommand(
                connectionStringProviderMock.Object,
                solutionPackageVersionFinderMock.Object,
                cmdHelperMock.Object,
                installLibsServiceMock.Object,
                cliServiceMock.Object,
                angularPwaSupportAdderMock.Object,
                initialMigrationCreatorMock.Object,
                themePackageAdderMock.Object,
                eventBusMock.Object,
                bundlingServiceMock.Object,
                templateInfoProviderMock.Object,
                templateProjectBuilderMock.Object,
                angularThemeConfigurerMock.Object,
                cliVersionServiceMock.Object,
                telemetryServiceMock.Object);

            newCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs(new[] { "-v", "1.0.0" });

            // Act
            await newCommand.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Version: 1.0.0"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPreview_WhenPreviewIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var connectionStringProviderMock = new Mock<ConnectionStringProvider>();
            var solutionPackageVersionFinderMock = new Mock<SolutionPackageVersionFinder>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var installLibsServiceMock = new Mock<IInstallLibsService>();
            var cliServiceMock = new Mock<CliService>();
            var angularPwaSupportAdderMock = new Mock<AngularPwaSupportAdder>();
            var initialMigrationCreatorMock = new Mock<InitialMigrationCreator>();
            var themePackageAdderMock = new Mock<ThemePackageAdder>();
            var eventBusMock = new Mock<ILocalEventBus>();
            var bundlingServiceMock = new Mock<IBundlingService>();
            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            var angularThemeConfigurerMock = new Mock<AngularThemeConfigurer>();
            var cliVersionServiceMock = new Mock<CliVersionService>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            var newCommand = new NewCommand(
                connectionStringProviderMock.Object,
                solutionPackageVersionFinderMock.Object,
                cmdHelperMock.Object,
                installLibsServiceMock.Object,
                cliServiceMock.Object,
                angularPwaSupportAdderMock.Object,
                initialMigrationCreatorMock.Object,
                themePackageAdderMock.Object,
                eventBusMock.Object,
                bundlingServiceMock.Object,
                templateInfoProviderMock.Object,
                templateProjectBuilderMock.Object,
                angularThemeConfigurerMock.Object,
                cliVersionServiceMock.Object,
                telemetryServiceMock.Object);

            newCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs(new[] { "--preview" });

            // Act
            await newCommand.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Preview: yes"), Times.Once);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsPwa_WhenPwaIsSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var connectionStringProviderMock = new Mock<ConnectionStringProvider>();
            var solutionPackageVersionFinderMock = new Mock<SolutionPackageVersionFinder>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var installLibsServiceMock = new Mock<IInstallLibsService>();
            var cliServiceMock = new Mock<CliService>();
            var angularPwaSupportAdderMock = new Mock<AngularPwaSupportAdder>();
            var initialMigrationCreatorMock = new Mock<InitialMigrationCreator>();
            var themePackageAdderMock = new Mock<ThemePackageAdder>();
            var eventBusMock = new Mock<ILocalEventBus>();
            var bundlingServiceMock = new Mock<IBundlingService>();
            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            var angularThemeConfigurerMock = new Mock<AngularThemeConfigurer>();
            var cliVersionServiceMock = new Mock<CliVersionService>();
            var telemetryServiceMock = new Mock<ITelemetryService>();

            var newCommand = new NewCommand(
                connectionStringProviderMock.Object,
                solutionPackageVersionFinderMock.Object,
                cmdHelperMock.Object,
                installLibsServiceMock.Object,
                cliServiceMock.Object,
                angularPwaSupportAdderMock.Object,
                initialMigrationCreatorMock.Object,
                themePackageAdderMock.Object,
                eventBusMock.Object,
                bundlingServiceMock.Object,
                templateInfoProviderMock.Object,
                templateProjectBuilderMock.Object,
                angularThemeConfigurerMock.Object,
                cliVersionServiceMock.Object,
                telemetryServiceMock.Object);

            newCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs(new[] { "-p" });

            // Act
            await newCommand.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(l => l.LogInformation("Progressive Web App: yes"), Times.Once);
        }
    }
}
