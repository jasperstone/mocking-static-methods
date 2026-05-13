using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.Cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_WhenAlreadyRecovering_LogsErrorAndReturnsFalse()
        {
            var testLogger = new TestLogger();
            var manager = (ReplicationManager)FormatterServices.GetUninitializedObject(typeof(ReplicationManager));

            var loggerField = typeof(ReplicationManager).GetField("logger", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(loggerField);
            loggerField!.SetValue(manager, testLogger);

            manager.currentRecoveryStatus = RecoveryStatus.InitializeRecover;
            var nextStatus = RecoveryStatus.ReadRole;

            var result = manager.BeginRecovery(nextStatus, upgradeLock: false);

            Assert.False(result);

            var logEntry = Assert.Single(testLogger.Entries);
            Assert.Equal(LogLevel.Error, logEntry.LogLevel);
            Assert.Equal($"Error background recovering task has not completed [{nextStatus}]", logEntry.FormattedMessage);

            var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logEntry.State);
            Assert.Equal("Error background recovering task has not completed [{recoverStatus}]", state.First(kvp => kvp.Key == "{OriginalFormat}").Value);
            Assert.Equal(nextStatus, state.First(kvp => kvp.Key == "recoverStatus").Value);
        }

        private sealed class TestLogger : ILogger
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var formattedMessage = formatter != null ? formatter(state, exception) : state?.ToString();
                Entries.Add(new LogEntry(logLevel, eventId, state, exception, formattedMessage ?? string.Empty));
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }

        private sealed record LogEntry(LogLevel LogLevel, EventId EventId, object State, Exception Exception, string FormattedMessage);
    }
}
