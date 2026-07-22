using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogModulesList()
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

            moduleProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);
            telemetryMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).ReturnsAsync(Mock.Of<IDisposable>());

            var command = new ListModulesCommand(moduleProviderMock.Object, telemetryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs
            {
                Options = new System.Collections.Generic.Dictionary<string, string>()
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Open Source Application Modules"))),
                Times.Once);
        }
    }
}
