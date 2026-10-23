using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_Call()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var mockTemplateBuilder = new Mock<TemplateProjectBuilder>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
            var mockCommandLineArgs = new CommandLineArgs
            {
                Target = "TestProject",
                Options = new CommandLineOptions()
            };

            // Setup minimal behavior for dependencies
            mockTemplateInfoProvider.Setup(p => p.GetDefaultAsync()).ReturnsAsync(new TemplateInfo { Name = "DefaultTemplate" });
            mockTemplateBuilder.Setup(b => b.BuildAsync(It.IsAny<ProjectBuildArgs>())).ReturnsAsync("result");
            // Instantiate the command with mocks
            var command = new NewCommand(
                null, null, null, null, null, null, null, null, null, 
                mockTemplateInfoProvider.Object, mockTemplateBuilder.Object, null, null, mockTelemetryService.Object);
            // Inject the mock logger
            typeof(NewCommand).GetProperty("Logger").SetValue(command, mockLogger.Object);

            // Act
            await command.ExecuteAsync(mockCommandLineArgs);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(It.Is<string>(s => s.Contains("Creating your project..."))),
                Times.AtLeastOnce);
        }
    }
}
