using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Internal.Telemetry;
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
        public async Task ExecuteAsync_ShouldLogInformation_WhenTieredOptionIsSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "new", "TestProject", "--tiered" });

            _templateInfoProviderMock.Setup(x => x.GetDefaultAsync())
                .ReturnsAsync(new TemplateInfo { Name = "default" });

            _templateProjectBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>()))
                .ReturnsAsync(new ProjectBuildResult(new byte[0], "TestProject"));

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tiered: yes")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
