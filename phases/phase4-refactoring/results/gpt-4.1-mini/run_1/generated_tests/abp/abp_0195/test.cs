using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ListModulesCommandTests
    {
        private class ModuleInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public bool IsPro { get; set; }
        }

        private class FakeModuleInfoProvider : ModuleInfoProvider
        {
            private readonly List<ModuleInfo> _modules;

            public FakeModuleInfoProvider(List<ModuleInfo> modules)
                : base(null, null, null, null)
            {
                _modules = modules;
            }

            public override Task<List<ModuleInfo>> GetModuleListAsync()
            {
                return Task.FromResult(_modules);
            }
        }

        private class FakeTelemetryService : ITelemetryService
        {
            public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);

            public IAsyncDisposable TrackActivityAsync(string activityName)
            {
                return new DummyAsyncDisposable();
            }

            private class DummyAsyncDisposable : IAsyncDisposable
            {
                public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
            }
        }

        [Fact]
        public async Task ExecuteAsync_LogsFreeModulesOnly_WhenNoProModulesOption()
        {
            // Arrange
            var modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "Module1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "Module2", DisplayName = "Module Two", IsPro = true }
            };

            var moduleInfoProvider = new FakeModuleInfoProvider(modules);
            var telemetryService = new FakeTelemetryService();

            var logger = new TestLogger<ListModulesCommand>();

            var command = new ListModulesCommand(moduleInfoProvider, telemetryService)
            {
                Logger = logger
            };

            var args = new CommandLineArgs("list-modules", null);

            // Act
            await command.ExecuteAsync(args);

            // Assert
            Assert.Contains("Open Source Application Modules", logger.LastLog);
            Assert.Contains("Module One", logger.LastLog);
            Assert.DoesNotContain("Commercial (Pro) Application Modules", logger.LastLog);
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

            var moduleInfoProvider = new FakeModuleInfoProvider(modules);
            var telemetryService = new FakeTelemetryService();

            var logger = new TestLogger<ListModulesCommand>();

            var command = new ListModulesCommand(moduleInfoProvider, telemetryService)
            {
                Logger = logger
            };

            var args = new CommandLineArgs("list-modules", null);
            args.Options["include-pro-modules"] = "true";

            // Act
            await command.ExecuteAsync(args);

            // Assert
            Assert.Contains("Open Source Application Modules", logger.LastLog);
            Assert.Contains("Module One", logger.LastLog);
            Assert.Contains("Commercial (Pro) Application Modules", logger.LastLog);
            Assert.Contains("Module Two", logger.LastLog);
        }

        private class TestLogger<T> : ILogger<T>
        {
            public string LastLog { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLog = formatter(state, exception);
            }
        }
    }
}
