using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Events;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.Utils;
using Volo.Abp.EventBus.Local;
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
        private readonly Mock<ILocalEventBus> _eventBusMock;
        private readonly NewCommand _newCommand;

        public NewCommandTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            _templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            _telemetryServiceMock = new Mock<ITelemetryService>();
            _eventBusMock = new Mock<ILocalEventBus>();

            _newCommand = new NewCommand(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _eventBusMock.Object,
                null,
                null,
                null,
                _templateInfoProviderMock.Object,
                _templateProjectBuilderMock.Object,
                null,
                null,
                _telemetryServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenTieredOptionIsProvided()
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

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Tiered: yes"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenTemplateIsProvided()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject",
                Options = new CommandLineOptions
                {
                    { Options.Template.Long, "TestTemplate" }
                }
            };

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Template: TestTemplate"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenDefaultTemplateIsUsed()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject"
            };

            _templateInfoProviderMock.Setup(x => x.GetDefaultAsync())
                .ReturnsAsync(new TemplateInfo { Name = "DefaultTemplate" });

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Template: DefaultTemplate"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenProjectIsSuccessfullyCreated()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject"
            };

            var projectArgs = new ProjectBuildArgs
            {
                OutputFolder = "TestOutputFolder"
            };

            _templateProjectBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult());

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("'TestProject' has been successfully created to 'TestOutputFolder'"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldAddActivity_WhenProjectIsSuccessfullyCreated()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject"
            };

            var projectArgs = new ProjectBuildArgs
            {
                OutputFolder = "TestOutputFolder"
            };

            _templateProjectBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult());

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _telemetryServiceMock.Verify(
                x => x.AddActivityAsync(ActivityNameConsts.AbpCliCommandsNewSolution, It.IsAny<Action<ActivityOptions>>()),
                Times.Once);
        }
    }
}
