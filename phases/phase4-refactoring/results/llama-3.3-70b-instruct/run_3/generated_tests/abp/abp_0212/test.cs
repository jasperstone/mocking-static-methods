using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var newCommand = new NewCommand(
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
                templateProjectBuilderMock.Object,
                telemetryServiceMock.Object
            );
            newCommand.Logger = loggerMock.Object;

            // Act
            await newCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
