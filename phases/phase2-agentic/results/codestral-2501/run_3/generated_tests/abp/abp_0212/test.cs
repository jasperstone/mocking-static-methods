using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class NewCommandTests
    {
        private readonly Mock<ILogger<NewCommand>> _loggerMock;
        private readonly Mock<ITemplateProjectBuilder> _templateProjectBuilderMock;
        private readonly Mock<ITelemetryService> _telemetryServiceMock;
        private readonly NewCommand _newCommand;

        public NewCommandTests()
        {
            _loggerMock = new Mock<ILogger<NewCommand>>();
            _templateProjectBuilderMock = new Mock<ITemplateProjectBuilder>();
            _telemetryServiceMock = new Mock<ITelemetryService>();

            _newCommand = new NewCommand(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
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
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tiered: yes")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
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
                    { Options.Template.Short, "test-template" }
                }
            };

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Template: test-template")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogInformation_WhenProjectNameIsProvided()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject"
            };

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Project name: TestProject")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
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

            _templateProjectBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult());

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("'TestProject' has been successfully created")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
