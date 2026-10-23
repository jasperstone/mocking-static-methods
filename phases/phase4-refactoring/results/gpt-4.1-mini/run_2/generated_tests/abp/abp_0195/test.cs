using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands
{
    public class ListModulesCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationWithFreeModulesOnly_WhenNoProModulesOption()
        {
            // Arrange
            var modules = new List<ModuleInfo>
            {
                new ModuleInfo("Module1", "Module 1", false),
                new ModuleInfo("Module2", "Module 2", false),
                new ModuleInfo("ProModule", "Pro Module", true)
            };

            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).Returns(new DummyAsyncDisposable());

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();
            // No options set, so no pro modules included

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Open Source Application Modules") && v.ToString().Contains("Module 1") && v.ToString().Contains("Module 2") && !v.ToString().Contains("Pro Module")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformationWithFreeAndProModules_WhenIncludeProModulesOption()
        {
            // Arrange
            var modules = new List<ModuleInfo>
            {
                new ModuleInfo("Module1", "Module 1", false),
                new ModuleInfo("Module2", "Module 2", false),
                new ModuleInfo("ProModule", "Pro Module", true)
            };

            var moduleInfoProviderMock = new Mock<ModuleInfoProvider>();
            moduleInfoProviderMock.Setup(m => m.GetModuleListAsync()).ReturnsAsync(modules);

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.TrackActivityAsync(It.IsAny<string>())).Returns(new DummyAsyncDisposable());

            var loggerMock = new Mock<ILogger<ListModulesCommand>>();

            var command = new ListModulesCommand(moduleInfoProviderMock.Object, telemetryServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();
            args.Options["include-pro-modules"] = "true";

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Open Source Application Modules") && v.ToString().Contains("Module 1") && v.ToString().Contains("Module 2") && v.ToString().Contains("Pro Module") && v.ToString().Contains("Commercial (Pro) Application Modules")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        private class DummyAsyncDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
        }
    }

    // Minimal stub for ModuleInfo to support the tests
    public class ModuleInfo
    {
        public string Name { get; }
        public string DisplayName { get; }
        public bool IsPro { get; }

        public ModuleInfo(string name, string displayName, bool isPro)
        {
            Name = name;
            DisplayName = displayName;
            IsPro = isPro;
        }
    }
}
