using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using System.Collections.Generic;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogModulesOutput()
        {
            // Arrange
            var moduleProviderMock = new Mock<ModuleInfoProvider>();
            var telemetryMock = new Mock<ITelemetryService>();
            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var modules = new[]
            {
                new ModuleInfo { Name = "ModuleA", DisplayName = "Module A", IsPro = false },
                new ModuleInfo { Name = "ModuleB", DisplayName = "Module B", IsPro = true }
            };

            moduleProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(new List<ModuleInfo>(modules));
            telemetryMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).ReturnsAsync(Mock.Of<IDisposable>());

            var command = new ListModulesCommand(moduleProviderMock.Object, telemetryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string>()
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Module A") && s.Contains("Module B"))),
                Times.Once);
        }
    }
}
