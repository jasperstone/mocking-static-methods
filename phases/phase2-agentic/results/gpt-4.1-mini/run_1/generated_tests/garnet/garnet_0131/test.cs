using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class MigrateSessionTests
    {
        // We will test the BeginAsyncMigrationTaskAsync method indirectly by triggering the failure paths
        // that cause logger.LogError calls on line 154 and others.

        // Since BeginAsyncMigrationTaskAsync is private, we trigger it via TryStartMigrationTaskAsync with transferOption != KEYS.

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);

            // Setup TrySetSlotRangesAsync to fail on first call (import state)
            migrateSession.TrySetSlotRangesAsyncFunc = (nodeid, state) =>
            {
                if (state == MigrateState.IMPORT)
                    return Task.FromResult(false);
                return Task.FromResult(true);
            };

            // Act
            var result = await migrateSession.TryStartMigrationTaskAsync();

            // Wait a bit for the background task to run
            await Task.Delay(100);

            // Assert
            // The logger should have been called with the error message about failing to set remote slots to import state
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set remote slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result.Success);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);

            // Setup TrySetSlotRangesAsync to succeed for import state
            migrateSession.TrySetSlotRangesAsyncFunc = (nodeid, state) => Task.FromResult(true);

            // Setup TryPrepareLocalForMigration to fail
            migrateSession.TryPrepareLocalForMigrationFunc = () => false;

            // Act
            var result = await migrateSession.TryStartMigrationTaskAsync();

            // Wait a bit for the background task to run
            await Task.Delay(100);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result.Success);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenReserveDestinationVectorSetsFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);

            migrateSession.TrySetSlotRangesAsyncFunc = (nodeid, state) => Task.FromResult(true);
            migrateSession.TryPrepareLocalForMigrationFunc = () => true;
            migrateSession.BumpAndWaitForEpochTransitionAsyncFunc = () => Task.FromResult(true);
            migrateSession.ReserveDestinationVectorSetsAsyncFunc = () => Task.FromResult(false);
            migrateSession.NamespacesCount = 1; // simulate namespaces present

            // Act
            var result = await migrateSession.TryStartMigrationTaskAsync();

            // Wait a bit for the background task to run
            await Task.Delay(100);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to reserve destination vector sets")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result.Success);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenMigrateSlotsDriverInlineFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);

            migrateSession.TrySetSlotRangesAsyncFunc = (nodeid, state) => Task.FromResult(true);
            migrateSession.TryPrepareLocalForMigrationFunc = () => true;
            migrateSession.BumpAndWaitForEpochTransitionAsyncFunc = () => Task.FromResult(true);
            migrateSession.ReserveDestinationVectorSetsAsyncFunc = () => Task.FromResult(true);
            migrateSession.NamespacesCount = 1;
            migrateSession.MigrateSlotsDriverInlineAsyncFunc = () => Task.FromResult(false);

            // Act
            var result = await migrateSession.TryStartMigrationTaskAsync();

            // Wait a bit for the background task to run
            await Task.Delay(100);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("MigrateSlotsDriver failed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result.Success);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
        }

        // Helper subclass to override dependencies and expose private members for testing
        private class TestableMigrateSession : MigrateSession
        {
            public TestableMigrateSession(ILogger logger)
            {
                this.logger = logger;
                this.transferOption = TransferOption.SLOTS; // to trigger BeginAsyncMigrationTaskAsync
                this.Status = MigrateState.INIT;
                this._sslots = new int[] { 1, 2, 3 };
                this.GetSlots = new int[] { 1, 2, 3 };
                this._cts = new CancellationTokenSource();
                this._timeout = TimeSpan.FromSeconds(1);
                this.clusterProvider = new TestClusterProvider();
            }

            public Func<string, MigrateState, Task<bool>> TrySetSlotRangesAsyncFunc { get; set; }
            public Func<bool> TryPrepareLocalForMigrationFunc { get; set; }
            public Func<Task<bool>> BumpAndWaitForEpochTransitionAsyncFunc { get; set; }
            public Func<Task<bool>> ReserveDestinationVectorSetsAsyncFunc { get; set; }
            public Func<Task<bool>> MigrateSlotsDriverInlineAsyncFunc { get; set; }
            public int NamespacesCount { get; set; }

            public new Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                if (TrySetSlotRangesAsyncFunc != null)
                    return TrySetSlotRangesAsyncFunc(nodeid, state);
                return base.TrySetSlotRangesAsync(nodeid, state);
            }

            public new bool TryPrepareLocalForMigration()
            {
                if (TryPrepareLocalForMigrationFunc != null)
                    return TryPrepareLocalForMigrationFunc();
                return base.TryPrepareLocalForMigration();
            }

            public new Task<bool> ReserveDestinationVectorSetsAsync()
            {
                if (ReserveDestinationVectorSetsAsyncFunc != null)
                    return ReserveDestinationVectorSetsAsyncFunc();
                return base.ReserveDestinationVectorSetsAsync();
            }

            public new Task<bool> MigrateSlotsDriverInlineAsync()
            {
                if (MigrateSlotsDriverInlineAsyncFunc != null)
                    return MigrateSlotsDriverInlineAsyncFunc();
                return base.MigrateSlotsDriverInlineAsync();
            }

            public new Task<bool> clusterProvider_BumpAndWaitForEpochTransitionAsync()
            {
                if (BumpAndWaitForEpochTransitionAsyncFunc != null)
                    return BumpAndWaitForEpochTransitionAsyncFunc();
                return clusterProvider.BumpAndWaitForEpochTransitionAsync();
            }

            // Override clusterProvider to use our test version
            public new TestClusterProvider clusterProvider { get; set; }

            // Override _namespaces property to simulate namespaces count
            private System.Collections.Generic.List<object> _namespacesBacking;
            public new System.Collections.Generic.List<object> _namespaces
            {
                get => _namespacesBacking ??= new System.Collections.Generic.List<object>(NamespacesCount);
                set => _namespacesBacking = value;
            }
        }

        private class TestClusterProvider
        {
            public TestStoreWrapper storeWrapper = new TestStoreWrapper();
            public TestClusterManager clusterManager = new TestClusterManager();

            public Task<bool> BumpAndWaitForEpochTransitionAsync()
            {
                return Task.FromResult(true);
            }
        }

        private class TestStoreWrapper
        {
            public TestStore store = new TestStore();
            public TestDatabase DefaultDatabase = new TestDatabase();
        }

        private class TestStore
        {
            public void PauseRevivification(TimeSpan timeout, CancellationToken token)
            {
                // no-op
            }
        }

        private class TestDatabase
        {
            public TestVectorManager VectorManager = new TestVectorManager();
        }

        private class TestVectorManager
        {
            public System.Collections.Generic.List<object> GetNamespacesForHashSlots(int[] slots)
            {
                return new System.Collections.Generic.List<object>();
            }
        }

        private class TestClusterManager
        {
            public void SuspendConfigMerge()
            {
                // no-op
            }
        }
    }
}
