using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Volo.Abp.Internal.Telemetry.Constants.Enums;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class NewCommandTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly Mock<ITemplateInfoProvider> _templateInfoProviderMock;
        private readonly Mock<TemplateProjectBuilder> _templateProjectBuilderMock;
        private readonly Mock<ITelemetryService> _telemetryServiceMock;
        private readonly NewCommand _newCommand;

        public NewCommandTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            _templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            _telemetryServiceMock = new Mock<ITelemetryService>();

            _newCommand = new NewCommand(
                Mock.Of<ConnectionStringProvider>(),
                Mock.Of<SolutionPackageVersionFinder>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<IInstallLibsService>(),
                Mock.Of<CliService>(),
                Mock.Of<AngularPwaSupportAdder>(),
                Mock.Of<InitialMigrationCreator>(),
                Mock.Of<ThemePackageAdder>(),
                Mock.Of<ILocalEventBus>(),
                Mock.Of<IBundlingService>(),
                _templateInfoProviderMock.Object,
                _templateProjectBuilderMock.Object,
                Mock.Of<AngularThemeConfigurer>(),
                Mock.Of<CliVersionService>(),
                _telemetryServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenCreatingProject()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject",
                Options = new CommandLineOptions
                {
                    { Options.Tiered.Long, "true" }
                }
            };

            _templateInfoProviderMock.Setup(x => x.GetDefaultAsync()).ReturnsAsync(new TemplateInfo { Name = "default" });
            _templateProjectBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>())).ReturnsAsync(new ProjectBuildResult());

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Creating your project..."), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("Project name: TestProject"), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("Tiered: yes"), Times.Once);
            _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.StartsWith("'TestProject' has been successfully created to '"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowException_WhenProjectNameIsMissing()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "",
                Options = new CommandLineOptions()
            };

            // Act & Assert
            await Assert.ThrowsAsync<CliUsageException>(() => _newCommand.ExecuteAsync(commandLineArgs));
        }
    }
}
