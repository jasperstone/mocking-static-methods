using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using System.Text;

namespace Volo.Abp.Cli.Tests
{
    public class ListModulesCommandTests
    {
        private class DummyLogger<T> : ILogger<T>
        {
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId,
                TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogMessage = formatter(state, exception);
                LastLogLevel = logLevel;
            }
        }

        private class DummyTelemetryService : ITelemetryService
        {
            public Task<IDisposable> TrackActivityAsync(string activityName)
            {
                return Task.FromResult((IDisposable)new DummyDisposable());
            }

            private class DummyDisposable : IDisposable
            {
                public void Dispose() { }
            }
        }

        private class DummyModuleInfoProvider
        {
            public List<ModuleInfo> Modules { get; set; } = new List<ModuleInfo>();

            public Task<List<ModuleInfo>> GetModuleListAsync()
            {
                return Task.FromResult(Modules);
            }
        }

        private class ModuleInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public bool IsPro { get; set; }
        }

        [Fact]
        public async Task ExecuteAsync_LogsCorrectInformation_ForOpenSourceModules()
        {
            // Arrange
            var moduleProvider = new DummyModuleInfoProvider();
            moduleProvider.Modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "mod1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "mod2", DisplayName = "Module Two", IsPro = false },
                new ModuleInfo { Name = "pro1", DisplayName = "Pro Module", IsPro = true }
            };

            var telemetryService = new DummyTelemetryService();
            var logger = new DummyLogger<ListModulesCommand>();
            var command = new ListModulesCommand(moduleProvider, telemetryService)
            {
                Logger = logger
            };

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string>()
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            Assert.Contains("Open Source Application Modules", logger.LastLogMessage);
            Assert.Contains("> Module One".PadRight(50), logger.LastLogMessage);
            Assert.Contains("> Module Two".PadRight(50), logger.LastLogMessage);
            Assert.DoesNotContain("Commercial (Pro) Application Modules", logger.LastLogMessage);
        }

        [Fact]
        public async Task ExecuteAsync_LogsCorrectInformation_WithProModulesIncluded()
        {
            // Arrange
            var moduleProvider = new DummyModuleInfoProvider();
            moduleProvider.Modules = new List<ModuleInfo>
            {
                new ModuleInfo { Name = "mod1", DisplayName = "Module One", IsPro = false },
                new ModuleInfo { Name = "pro1", DisplayName = "Pro Module", IsPro = true }
            };

            var telemetryService = new DummyTelemetryService();
            var logger = new DummyLogger<ListModulesCommand>();
            var command = new ListModulesCommand(moduleProvider, telemetryService)
            {
                Logger = logger
            };

            var args = new CommandLineArgs
            {
                Options = new Dictionary<string, string> { { "include-pro-modules", "" } }
            };

            // Act
            await command.ExecuteAsync(args);

            // Assert
            Assert.Contains("Open Source Application Modules", logger.LastLogMessage);
            Assert.Contains("> Module One".PadRight(50), logger.LastLogMessage);
            Assert.Contains("Commercial (Pro) Application Modules", logger.LastLogMessage);
            Assert.Contains("> Pro Module".PadRight(50), logger.LastLogMessage);
        }
    }
}
