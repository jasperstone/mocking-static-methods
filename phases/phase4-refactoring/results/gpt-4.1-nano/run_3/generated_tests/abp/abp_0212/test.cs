using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using System.Collections.Generic;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_Called()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
            var mockTemplateProjectBuilder = new Mock<TemplateProjectBuilder>();

            // Setup default behaviors
            mockTemplateInfoProvider.Setup(p => p.GetDefaultAsync()).ReturnsAsync(new { Name = "DefaultTemplate" });
            mockTemplateProjectBuilder.Setup(b => b.BuildAsync(It.IsAny<ProjectBuildArgs>())).ReturnsAsync("result");

            var command = new NewCommand(
                null, null, null, null, null, null, null, null, null,
                mockTemplateInfoProvider.Object,
                mockTemplateProjectBuilder.Object,
                null, null, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            var mockArgs = new Mock<CommandLineArgs>();
            mockArgs.Setup(a => a.Target).Returns("TestProject");
            mockArgs.Setup(a => a.Options).Returns(new Dictionary<string, string>());

            // Act
            await command.ExecuteAsync(mockArgs.Object);

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Creating your project..."))), Times.AtLeastOnce);
            mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Project name: TestProject"))), Times.AtLeastOnce);
        }
    }
}
