using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.Utils;
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
            var commandLineArgs = new CommandLineArgs("new", "TestProject", new AbpCommandLineOptions
            {
                { Volo.Abp.Cli.Options.Tiered.Long, "true" }
            });

            _templateInfoProviderMock.Setup(x => x.GetDefaultAsync()).ReturnsAsync(new TemplateInfo { Name = "default" });
            _templateProjectBuilderMock.Setup(x => x.BuildAsync(It.IsAny<ProjectBuildArgs>())).ReturnsAsync(new ProjectBuildResult(new byte[0], "output"));

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Tiered: yes"),
                Times.Once);
        }
    }
}
