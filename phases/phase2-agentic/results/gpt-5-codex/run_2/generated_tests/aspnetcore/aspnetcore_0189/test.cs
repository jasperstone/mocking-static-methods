using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class ApplicationDeployerTests
    {
        [Fact]
        public void TriggerHostShutdown_LogsInformationAndCancelsHostProcess()
        {
            var logger = new RecordingLogger();
            var deployer = TestApplicationDeployer.Create(logger);
            using var cts = new CancellationTokenSource();

            deployer.InvokeTriggerHostShutdown(cts);

            Assert.True(cts.IsCancellationRequested);
            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Information, entry.Level);
            Assert.Equal("Host process shutting down.", entry.Message);
        }

        private sealed class RecordingLogger : ILogger
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString();
                Entries.Add(new LogEntry(logLevel, message ?? string.Empty));
            }

            public sealed record LogEntry(LogLevel Level, string Message);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }

        private sealed class TestApplicationDeployer : ApplicationDeployer
        {
            private TestApplicationDeployer()
                : base(null!, null!)
            {
            }

            public static TestApplicationDeployer Create(ILogger logger)
            {
                var deployer = (TestApplicationDeployer)FormatterServices.GetUninitializedObject(typeof(TestApplicationDeployer));
                var loggerField = typeof(ApplicationDeployer).GetField("<Logger>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                if (loggerField is null)
                {
                    throw new InvalidOperationException("Logger backing field not found.");
                }

                loggerField.SetValue(deployer, logger);
                return deployer;
            }

            public void InvokeTriggerHostShutdown(CancellationTokenSource source)
            {
                TriggerHostShutdown(source);
            }

            public override Task<DeploymentResult> DeployAsync()
                => throw new NotSupportedException();

            public override void Dispose()
            {
            }
        }
    }
}
