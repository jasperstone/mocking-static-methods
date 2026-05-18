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
        public async Task ExecuteAsync_LogsFreeModulesOnly_WhenNoProModulesOption()
        {
            // Arrange
            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "Module1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "Module2", DisplayName = "Module Two", IsPro = true }
            };

            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>(null, null, null, null);
            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).Returns(Task.FromResult(modules));

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).Returns(Task.FromResult<IAsyncDisposable>(new DummyAsyncDisposable()));

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs("list-modules", null);

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Open Source Application Modules") && v.ToString().Contains("Module One")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Should not contain pro modules
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Commercial (Pro) Application Modules")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_LogsFreeAndProModules_WhenIncludeProModulesOption()
        {
            // Arrange
            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "Module1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "Module2", DisplayName = "Module Two", IsPro = true }
            };

            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>(null, null, null, null);
            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).Returns(Task.FromResult(modules));

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).Returns(Task.FromResult<IAsyncDisposable>(new DummyAsyncDisposable()));

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs("list-modules", null);
            args.Options["include-pro-modules"] = "true";

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Open Source Application Modules") && v.ToString().Contains("Module One")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Commercial (Pro) Application Modules") && v.ToString().Contains("Module Two")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class DummyAsyncDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
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
