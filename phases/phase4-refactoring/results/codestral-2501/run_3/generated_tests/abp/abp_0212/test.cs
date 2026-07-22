using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Templates;
using Volo.Abp.Cli.ProjectBuilding.Templates.Module;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
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
        public async Task ExecuteAsync_LogsTieredYes_WhenTieredOptionIsPresent()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(
                "TestProject",
                new AbpCommandLineOptions
                {
                    { "--tiered", "true" }
                });

            // Act
            await _newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Tiered: yes"),
                Times.Once);
        }
    }
}
