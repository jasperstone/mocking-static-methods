using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using System.Text;
using Moq;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_Log_Correct_Output_With_IncludeProModules()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ListModulesCommand>>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockModuleProvider = new Mock<ModuleInfoProvider>();

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "ModuleA", DisplayName = "Module A", IsPro = false },
                new ModuleInfo { Name = "ModuleB", DisplayName = "Module B", IsPro = true }
            };

            mockModuleProvider.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);
            var command = new ListModulesCommand(mockModuleProvider.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string> { { "include-pro-modules", "" } }
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Open Source Application Modules") && s.Contains("Commercial (Pro) Application Modules"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_Log_Correct_Output_Without_IncludeProModules()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ListModulesCommand>>();
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockModuleProvider = new Mock<ModuleInfoProvider>();

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "ModuleA", DisplayName = "Module A", IsPro = false },
                new ModuleInfo { Name = "ModuleB", DisplayName = "Module B", IsPro = true }
            };

            mockModuleProvider.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);
            var command = new ListModulesCommand(mockModuleProvider.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string>() // no include-pro-modules
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Open Source Application Modules") && !s.Contains("Commercial (Pro) Application Modules"))),
                Times.Once);
        }
    }

    // Mock classes for dependencies
    public class CommandLineArgs
    {
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
    }

    public class ModuleInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsPro { get; set; }
    }
}
