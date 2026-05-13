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
            public static string[] GetRange(int[] slots) => new string[] { "slotRange" };
        }

        private class DummyStoreWrapper
        {
            public DummyVectorManager VectorManager { get; } = new DummyVectorManager();
        }

        private class DummyVectorManager
        {
            public System.Collections.Generic.List<string> GetNamespacesForHashSlots(int[] slots) => new System.Collections.Generic.List<string> { "namespace1" };
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public DummyClusterManager clusterManager = new DummyClusterManager();
            public DummyMigrationManager migrationManager = new DummyMigrationManager();

            public Task<bool> BumpAndWaitForEpochTransitionAsync() => Task.FromResult(true);
            public Task<bool> SuspendConfigMerge() => Task.FromResult(true);
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

        private class DummyMigrateSession : MigrateSession
        {
            public DummyMigrateSession()
            {
                // Initialize required members
                this.logger = new DummyLogger();
                this.migrateOperation = new[] { new { Client = new DummyClient() } };
                this._sslots = new[] { 1, 2, 3 };
                this._timeout = TimeSpan.FromSeconds(1);
                this._cts = new CancellationTokenSource();
                this.Status = MigrateState.INIT;
                this.clusterProvider = new DummyClusterProvider();
            }

            public DummyClusterProvider clusterProvider;
            public DummyLogger logger;
            public dynamic[] migrateOperation;
            public int[] _sslots;
            public TimeSpan _timeout;
            public CancellationTokenSource _cts;
            public MigrateState Status;

            public override Task<bool> CheckConnectionAsync(DummyClient client)
            {
                return Task.FromResult(true);
            }

            public override string[] GetSlots => _sslots;

            public override string GetSourceNodeId => "node1";

            public override void ResetLocalSlot()
            {
                // No-op for test
            }

            public override bool TryPrepareLocalForMigration() => true;

            public override Task<bool> MigrateKeysAsync() => Task.FromResult(true);

            public override Task<bool> MigrateSlotsDriverInlineAsync() => Task.FromResult(true);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_Should_LogError_When_ResultIsNotOK()
        {
            // Arrange
            var session = new DummyMigrateSession();
            var loggerMock = new Mock<ILogger>();
            session.logger = loggerMock.Object;

            var clientMock = new DummyClient
            {
                SetSlotRangeAsync = (stateBytes, nodeid, slots) => Task.FromResult("ErrorResult")
            };

            session.migrateOperation = new[] { new { Client = clientMock } };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error: ErrorResult")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_Should_LogError_When_OperationCanceledException()
        {
            // Arrange
            var session = new DummyMigrateSession();
            var loggerMock = new Mock<ILogger>();
            session.logger = loggerMock.Object;

            var clientMock = new DummyClient
            {
                SetSlotRangeAsync = (stateBytes, nodeid, slots) => throw new OperationCanceledException()
            };

            session.migrateOperation = new[] { new { Client = clientMock } };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange operation timed out or was cancelled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_Should_LogError_When_Exception()
        {
            // Arrange
            var session = new DummyMigrateSession();
            var loggerMock = new Mock<ILogger>();
            session.logger = loggerMock.Object;

            var clientMock = new DummyClient
            {
                SetSlotRangeAsync = (stateBytes, nodeid, slots) => throw new Exception("TestException")
            };

            session.migrateOperation = new[] { new { Client = clientMock } };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during SetSlotRange for slots")),
                It.Is<Exception>(ex => ex.Message == "TestException"),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
