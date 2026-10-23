using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace MigrationTests
{
    public class MigrationDriverTests
    {
        private class DummyClient
        {
            public virtual Task<string> SetSlotRange(byte[] stateBytes, string nodeId, int[] slots)
            {
                return Task.FromResult("OK");
            }
        }

        private class DummyClusterManager
        {
            public static int[] GetRange(int[] slots) => slots;
        }

        private class DummyStoreWrapper
        {
            public class DummyStore
            {
                public void PauseRevivification(TimeSpan timeout, System.Threading.CancellationToken token) { }
            }

            public DummyStore store = new DummyStore();
            public class DummyDatabase
            {
                public VectorManager VectorManager => new VectorManager();
            }

            public DummyDatabase DefaultDatabase => new DummyDatabase();

            public class VectorManager
            {
                public System.Collections.Generic.List<string> GetNamespacesForHashSlots(int[] slots) => new System.Collections.Generic.List<string>();
            }
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public DummyClusterManager clusterManager = new DummyClusterManager();
            public DummyMigrationManager migrationManager = new DummyMigrationManager();

            public Task<bool> BumpAndWaitForEpochTransitionAsync() => Task.FromResult(true);
        }

        private class DummyMigrationManager
        {
            public bool TryRemoveMigrationTask(MigrateSession session) => true;
        }

        private class DummyLogger : ILogger<MigrateSession>
        {
            public System.Collections.Generic.List<string> ErrorLogs = new();

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    ErrorLogs.Add(formatter(state, exception));
                }
            }
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenResultIsNotOk()
        {
            // Arrange
            var mockClient = new Mock<DummyClient>();
            mockClient.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int[]>()))
                      .ReturnsAsync("ErrorResult");

            var logger = new DummyLogger();

            var session = new MigrateSession(/* initialize with dependencies */);
            // Inject dependencies
            session.logger = logger;
            session.migrateOperation = new[] { new { Client = mockClient.Object } };
            session._sslots = new[] { 1, 2, 3 };
            session._timeout = TimeSpan.FromSeconds(1);
            session._cts = new System.Threading.CancellationTokenSource();
            session.Status = MigrateState.IMPORT;

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            Assert.Contains("SetSlotRange error: ErrorResult", logger.ErrorLogs);
        }
    }
}
