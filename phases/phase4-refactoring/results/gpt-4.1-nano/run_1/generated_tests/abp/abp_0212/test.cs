using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_CallLine95()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
            var mockTemplateProjectBuilder = new Mock<TemplateProjectBuilder>();
            var mockEventBus = new Mock<ILocalEventBus>();

            var command = new NewCommand(
                null, null, null, null, null, null, null, null, mockEventBus.Object,
                null, mockTemplateInfoProvider.Object, mockTemplateProjectBuilder.Object, null, null, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject",
                Options = new Dictionary<string, string>()
            };

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Creating your project..."))), Times.AtLeastOnce);
        }
    }
}
