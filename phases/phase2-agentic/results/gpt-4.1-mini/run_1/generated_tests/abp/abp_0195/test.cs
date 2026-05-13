using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsModulesList_WithoutProModules()
        {
            // Arrange
            var mockModuleInfoProvider = new Mock<ModuleInfoProvider>(null, null, null, null);
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockLogger = new Mock<ILogger<ListModulesCommand>>();
            var mockActivity = new Mock<IAsyncDisposable>();

            mockTelemetryService
                .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(mockActivity.Object);

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "Module1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "Module2", DisplayName = "Module Two", IsPro = true }
            };

            mockModuleInfoProvider
                .Setup(m => m.GetModuleListAsync())
                .ReturnsAsync(modules);

            var command = new ListModulesCommand(mockModuleInfoProvider.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            var args = new CommandLineArgs(new string[0]);

            // Act
            await command.ExecuteAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Open Source Application Modules") && v.ToString().Contains("Module One")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Pro module should not be included
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Module Two")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_LogsModulesList_WithProModules()
        {
            // Arrange
            var mockModuleInfoProvider = new Mock<ModuleInfoProvider>(null, null, null, null);
            var mockTelemetryService = new Mock<ITelemetryService>();
            var mockLogger = new Mock<ILogger<ListModulesCommand>>();
            var mockActivity = new Mock<IAsyncDisposable>();

            mockTelemetryService
                .Setup(t => t.TrackActivityAsync(It.IsAny<string>()))
                .ReturnsAsync(mockActivity.Object);

            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "Module1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "Module2", DisplayName = "Module Two", IsPro = true }
            };

            mockModuleInfoProvider
                .Setup(m => m.GetModuleListAsync())
                .ReturnsAsync(modules);

            var command = new ListModulesCommand(mockModuleInfoProvider.Object, mockTelemetryService.Object)
            {
                Logger = mockLogger.Object
            };

            var args = new CommandLineArgs(new[] { "--include-pro-modules" });

            // Act
            await command.ExecuteAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("Open Source Application Modules") &&
                        v.ToString().Contains("Module One") &&
                        v.ToString().Contains("Commercial (Pro) Application Modules") &&
                        v.ToString().Contains("Module Two")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal ModuleInfo class for testing
    public class ModuleInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsPro { get; set; }
    }
}
