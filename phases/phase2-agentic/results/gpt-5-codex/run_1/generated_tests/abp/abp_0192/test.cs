using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class HelpCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformationFromCommandUsage_WhenTargetCommandExists()
        {
            // Arrange
            var cliOptions = new AbpCliOptions();
            cliOptions.Commands["fake"] = typeof(FakeConsoleCommand);

            var fakeCommand = new FakeConsoleCommand("usage info from fake command");
            var serviceProvider = new TrackingServiceProvider();
            serviceProvider.AddService(typeof(FakeConsoleCommand), fakeCommand);

            var scope = new TrackingServiceScope(serviceProvider);
            var scopeFactory = new TrackingServiceScopeFactory(scope);

            var logger = new TestLogger<HelpCommand>();

            var helpCommand = new HelpCommand(Options.Create(cliOptions), scopeFactory)
            {
                Logger = logger
            };

            // Act
            await helpCommand.ExecuteAsync(new CommandLineArgs("help", "fake"));

            // Assert
            Assert.Contains(logger.Logs, entry => entry.Level == LogLevel.Information && entry.Message == fakeCommand.ExpectedUsageInfo);
            Assert.Equal(1, fakeCommand.UsageInfoCallCount);
            Assert.Equal(1, scopeFactory.CreateScopeCallCount);
            Assert.True(serviceProvider.GetServiceCallCount >= 1);
            Assert.True(scope.DisposeCallCount >= 1);
        }

        private class FakeConsoleCommand : IConsoleCommand
        {
            public string ExpectedUsageInfo { get; }
            public int UsageInfoCallCount { get; private set; }

            public FakeConsoleCommand(string expectedUsageInfo)
            {
                ExpectedUsageInfo = expectedUsageInfo;
            }

            public Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                return Task.CompletedTask;
            }

            public string GetUsageInfo()
            {
                UsageInfoCallCount++;
                return ExpectedUsageInfo;
            }

            public static string GetShortDescription() => "Fake command used for testing.";
        }

        private class TrackingServiceScopeFactory : IServiceScopeFactory
        {
            private readonly IServiceScope _scope;

            public TrackingServiceScopeFactory(IServiceScope scope)
            {
                _scope = scope;
            }

            public int CreateScopeCallCount { get; private set; }

            public IServiceScope CreateScope()
            {
                CreateScopeCallCount++;
                return _scope;
            }
        }

        private class TrackingServiceScope : IServiceScope
        {
            public TrackingServiceScope(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public IServiceProvider ServiceProvider { get; }

            public int DisposeCallCount { get; private set; }

            public void Dispose()
            {
                DisposeCallCount++;
            }
        }

        private class TrackingServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _services = new();

            public int GetServiceCallCount { get; private set; }

            public void AddService(Type serviceType, object instance)
            {
                _services[serviceType] = instance;
            }

            public object GetService(Type serviceType)
            {
                GetServiceCallCount++;
                return _services.TryGetValue(serviceType, out var service) ? service : null;
            }
        }

        private class TestLogger<T> : ILogger<T>
        {
            public List<LogEntry> Logs { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Logs.Add(new LogEntry(logLevel, message));
            }

            public record LogEntry(LogLevel Level, string Message);

            private class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }
}
