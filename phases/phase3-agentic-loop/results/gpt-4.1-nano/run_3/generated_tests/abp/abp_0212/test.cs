using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_CalledOnLine95()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
            var mockTemplateProjectBuilder = new Mock<TemplateProjectBuilder>();

            var command = new NewCommand(
                null, null, null, null, null, null, null, null, null, null, null,
                mockTemplateProjectBuilder.Object, null, mockTelemetryService.Object);

            // Inject the mock logger
            command.GetType().GetProperty("Logger").SetValue(command, mockLogger.Object);

            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject",
                Options = new Dictionary<string, string>()
            };

            // Setup necessary method mocks
            mockTemplateInfoProvider.Setup(p => p.GetDefaultAsync()).ReturnsAsync(new { Name = "DefaultTemplate" });
            mockTemplateProjectBuilder.Setup(b => b.BuildAsync(It.IsAny<object>())).ReturnsAsync("result");
            // Call the method
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Creating your project..."),
                Times.Once);
        }
    }
}
