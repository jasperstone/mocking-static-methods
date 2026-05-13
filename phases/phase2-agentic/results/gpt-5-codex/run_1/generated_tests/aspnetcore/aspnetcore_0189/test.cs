using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class ApplicationDeployerTests
    {
        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancelsToken()
        {
            var loggerProvider = new TestLoggerProvider();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(loggerProvider));

            var parameters = new DeploymentParameters(ServerType.Kestrel, RuntimeFlavor.CoreClr, RuntimeArchitecture.x64)
            {
                ApplicationPath = AppContext.BaseDirectory
            };

            var deployer = new TestApplicationDeployer(parameters, loggerFactory);
            using var cts = new CancellationTokenSource();

            deployer.InvokeTriggerHostShutdown(cts);

            Assert.True(cts.IsCancellationRequested);

            Assert.Contains(loggerProvider.Entries, entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == "Host process shutting down.");

            deployer.Dispose();
        }

        private sealed class TestApplicationDeployer : ApplicationDeployer
        {
            public TestApplicationDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            public override Task<DeploymentResult> DeployAsync()
                => Task.FromResult<DeploymentResult>(null);

            public override void Dispose()
            {
            }

            public void InvokeTriggerHostShutdown(CancellationTokenSource hostShutdownSource)
                => TriggerHostShutdown(hostShutdownSource);
        }

        private sealed class TestLoggerProvider : ILoggerProvider
        {
            private readonly List<LogEntry> _entries = new();

            public IReadOnlyList<LogEntry> Entries => _entries;

            public ILogger CreateLogger(string categoryName) => new TestLogger(_entries);

            public void Dispose()
            {
            }

            private sealed class TestLogger : ILogger
            {
                private readonly List<LogEntry> _entries;

                public TestLogger(List<LogEntry> entries)
                {
                    _entries = entries;
                }

                public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    var message = formatter != null ? formatter(state, exception) : state?.ToString();
                    _entries.Add(new LogEntry(logLevel, message));
                }
            }
        }

        private sealed record LogEntry(LogLevel LogLevel, string Message);

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();

            private NullScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
