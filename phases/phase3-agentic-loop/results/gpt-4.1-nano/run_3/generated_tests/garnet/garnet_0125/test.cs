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
            public static string[] GetRange(int[] slots) => new string[] { "slotRange" };
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
            public DummyVectorManager VectorManager = new DummyVectorManager();
            public DummyStore store = new DummyStore();

            public class DummyStore
            {
                public DummyRevivification store = new DummyRevivification();
            }

            public class DummyRevivification
            {
                public void PauseRevivification(TimeSpan timeout, CancellationToken token) { }
            }
        }

        private class DummyVectorManager
        {
            public System.Collections.Generic.List<string> GetNamespacesForHashSlots(int[] slots) => new System.Collections.Generic.List<string> { "namespace1" };
        }

        private class DummyMigrationManager
        {
            public bool TryRemoveMigrationTask(object task) => true;
        }

        private class DummyCluster
        {
            public void SuspendConfigMerge() { }
        }

        private class DummyLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        }

        [Fact]
        public async Task LogTrace_Called_With_Correct_Parameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var dummyClient = new DummyClient
            {
                SetSlotRangeAsync = (state, nodeid, slots) => Task.FromResult("OK")
            };
            var session = new MigrateSession
            {
                logger = mockLogger.Object,
                migrateOperation = new[] { new { Client = dummyClient } },
                _sslots = new[] { 1, 2, 3 },
                _timeout = TimeSpan.FromSeconds(10),
                _cts = new CancellationTokenSource(),
                Status = MigrateState.STABLE,
                clusterProvider = new DummyClusterProvider(),
                _slotRanges = new[] { 1, 2, 3 }
            };

            // Act
            await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            mockLogger.Verify(
                x => x.LogTrace(
                    It.Is<string>(s => s.Contains("Sending CLUSTER SETSLOTRANGE")),
                    MigrateState.STABLE,
                    "null",
                    It.IsAny<string[]>()),
                Times.Once);
        }
    }
}
