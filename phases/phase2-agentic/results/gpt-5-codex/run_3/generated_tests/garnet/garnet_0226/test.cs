using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.Tests.Cluster.Replication
{
    public class ReplicationManagerLoggingTests
    {
        private sealed class TestLogger<T> : ILogger<T>, IDisposable
        {
            private readonly List<(LogLevel Level, EventId EventId, string Message, Exception Exception)> _entries = new();

            public IReadOnlyList<(LogLevel Level, EventId EventId, string Message, Exception Exception)> Entries => _entries;

            public IDisposable BeginScope<TState>(TState state) => this;

            public void Dispose()
            {
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                _entries.Add((logLevel, eventId, message, exception));
            }
        }

        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundRetrievalInformation()
        {
            // Arrange
            var replicationManagerType = typeof(Garnet.server.Server).Assembly.GetType("Garnet.cluster.ReplicationManager", true);
            var optionsType = typeof(Garnet.server.Server).Assembly.GetType("Garnet.cluster.ReplicateSyncOptions", true);

            var replicationManager = FormatterServices.GetUninitializedObject(replicationManagerType);
            var loggerField = replicationManagerType.GetField("logger", BindingFlags.Instance | BindingFlags.NonPublic);
            var logger = new TestLogger<object>();
            loggerField!.SetValue(replicationManager, logger);

            var replicateOptions = Activator.CreateInstance(optionsType);
            optionsType.GetProperty("Background")!.SetValue(replicateOptions, false);
            optionsType.GetProperty("TryAddReplica")!.SetValue(replicateOptions, false);
            optionsType.GetProperty("Force")!.SetValue(replicateOptions, false);
            optionsType.GetProperty("UpgradeLock")!.SetValue(replicateOptions, false);
            optionsType.GetProperty("NodeId")!.SetValue(replicateOptions, Guid.Empty);

            var method = replicationManagerType.GetMethod("TryReplicateDiskbasedSyncAsync", BindingFlags.Instance | BindingFlags.Public);
            var session = (object)null;

            // Act
            var task = (Task<(bool Success, ReadOnlyMemory<byte> ErrorMessage)>)method!.Invoke(
                replicationManager,
                new[] { session, replicateOptions });

            var result = await task;

            // Assert
            Assert.False(result.Success);
            Assert.Contains(
                logger.Entries,
                entry => entry.Level == LogLevel.Information && entry.Message == "Initiating foreground checkpoint retrieval");
        }
    }
}
