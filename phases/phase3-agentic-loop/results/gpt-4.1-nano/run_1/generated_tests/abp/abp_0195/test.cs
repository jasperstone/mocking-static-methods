using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Moq;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_Log_Correct_Output_IncludingProModules()
        {
            // Arrange
            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "ModuleA", DisplayName = "Module A", IsPro = false },
                new ModuleInfo { Name = "ModuleB", DisplayName = "Module B", IsPro = true }
            };

            var moduleProviderMock = new Mock<ModuleInfoProvider>();
            moduleProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            var telemetryMock = new Mock<ITelemetryService>();
            telemetryMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).ReturnsAsync(Mock.Of<IDisposable>());

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleProviderMock.Object, telemetryMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string> { { "include-pro-modules", "" } }
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Open Source Application Modules"))), Times.Once);
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Commercial (Pro) Application Modules"))), Times.Once);
        }
    }

    // Mock classes for dependencies
    public class ModuleInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsPro { get; set; }
    }
}
