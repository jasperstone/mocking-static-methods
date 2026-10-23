using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var telemetryServiceMock = new Mock<Volo.Abp.Internal.Telemetry.ITelemetryService>();
            var cliVersionServiceMock = new Mock<Volo.Abp.Cli.Version.CliVersionService>();
            var templateProjectBuilderMock = new Mock<Volo.Abp.Cli.ProjectBuilding.TemplateProjectBuilder>();
            var newCommand = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, null, null, templateProjectBuilderMock.Object, cliVersionServiceMock.Object, telemetryServiceMock.Object, loggerMock.Object);

            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Target = "MyProject";

            // Act
            await newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
