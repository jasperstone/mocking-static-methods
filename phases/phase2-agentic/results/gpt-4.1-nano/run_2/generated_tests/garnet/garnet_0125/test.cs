using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
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
            public static string[] GetRange(int[] slots) => new string[] { "0-100" };
        }

        private class DummyStoreWrapper
        {
            public DummyVectorManager VectorManager { get; } = new DummyVectorManager();
        }

        private class DummyVectorManager
        {
            public System.Collections.Generic.List<string> GetNamespacesForHashSlots(int[] slots) => new System.Collections.Generic.List<string> { "ns" };
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public DummyClusterManager clusterManager = new DummyClusterManager();
            public DummyMigrationManager migrationManager = new DummyMigrationManager();

            public Task<bool> BumpAndWaitForEpochTransitionAsync() => Task.FromResult(true);
            public Task<bool> ReserveDestinationVectorSetsAsync() => Task.FromResult(true);
            public void storeWrapper_PauseRevivification(TimeSpan timeout, CancellationToken token) { }
        }

        private class DummyMigrationManager
        {
            public bool TryRemoveMigrationTask(object task) => true;
        }

        private class DummyLogger : ILogger
        {
            public string LastLog { get; private set; }
            public LogLevel LastLevel { get; private set; }
            public Exception LastException { get; private set; }
            public object[] LastState { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLog = formatter(state, exception);
                LastLevel = logLevel;
                LastException = exception;
                LastState = new object[] { state };
            }
        }

        [Fact]
        public async Task LogTrace_Called_On_Successful_SetSlotRange()
        {
            // Arrange
            var logger = new DummyLogger();
            var session = new MigrateSession
            {
                logger = logger,
                Status = MigrateState.IMPORT,
                _timeout = TimeSpan.FromSeconds(10),
                _cts = new CancellationTokenSource(),
                _sslots = new int[] { 0, 1, 2 },
                _slotRanges = new string[] { "0-100" },
                migrateOperation = new[] { new { Client = new DummyClient() } },
                ClusterManager = new DummyClusterManager(),
                clusterProvider = new DummyClusterProvider(),
                CheckConnectionAsync = client => Task.FromResult(true),
                ClusterManager = new DummyClusterManager(),
            };
            // Override client to simulate success
            var client = new DummyClient();
            client.SetSlotRangeAsync = (stateBytes, nodeid, slots) => Task.FromResult("OK");
            session.migrateOperation[0] = new { Client = client };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            Assert.True(result);
            Assert.Contains("Sending CLUSTER SETSLOTRANGE", logger.LastLog);
            Assert.Contains("SETSLOT", logger.LastLog);
        }
    }
}
