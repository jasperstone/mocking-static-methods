using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        private class DummyClient
        {
            public Func<string, string, string[], Task<string>> SetSlotRangeAsync { get; set; }
            public Task<string> SetSlotRange(string stateBytes, string nodeid, string[] slots)
            {
                return SetSlotRangeAsync?.Invoke(stateBytes, nodeid, slots) ?? Task.FromResult("OK");
            }
        }

        private class DummyClusterManager
        {
            public static string[] GetRange(int[] slots) => new[] { "0-100" };
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public DummyClusterManager clusterManager = new DummyClusterManager();
            public DummyMigrationManager migrationManager = new DummyMigrationManager();

            public Task<bool> BumpAndWaitForEpochTransitionAsync() => Task.FromResult(true);
        }

        private class DummyStoreWrapper
        {
            public DummyVectorManager VectorManager { get; } = new DummyVectorManager();
            public DummyStore store { get; } = new DummyStore();
        }

        private class DummyVectorManager
        {
            public System.Collections.Generic.List<string> GetNamespacesForHashSlots(int[] slots) => new System.Collections.Generic.List<string> { "namespace1" };
        }

        private class DummyStore
        {
            public DummyStore PauseRevivification(TimeSpan timeout, CancellationToken token) => this;
        }

        private class DummyMigrationManager
        {
            public bool TryRemoveMigrationTask(object task) => true;
        }

        private class DummyLogger : ILogger
        {
            public readonly System.Collections.Generic.List<string> Logs = new();

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Logs.Add(formatter(state, exception));
            }
        }

        [Fact]
        public async Task LogTrace_Called_With_Correct_Message()
        {
            // Arrange
            var logger = new DummyLogger();
            var mockClient = new DummyClient
            {
                SetSlotRangeAsync = (stateBytes, nodeid, slots) => Task.FromResult("OK")
            };
            var session = new MigrateSession
            {
                logger = logger,
                migrateOperation = new[] { new { Client = mockClient } },
                _sslots = new[] { 0, 1, 2 },
                _timeout = TimeSpan.FromSeconds(10),
                _cts = new CancellationTokenSource(),
                Status = MigrateState.STABLE,
                clusterProvider = new DummyClusterProvider(),
                _slotRanges = new[] { 0, 1, 2 }
            };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            Assert.True(result);
            Assert.Contains("Sending CLUSTER SETSLOTRANGE", logger.Logs);
        }
    }
}
