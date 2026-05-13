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
        // We will test the BeginAsyncMigrationTaskAsync method indirectly by calling TryStartMigrationTaskAsync
        // with transferOption != KEYS to trigger the background task.
        // We want to cover the LogError call on line 154 (Failed to set local slots {slots} to migrate state).

        // To do this, we will mock dependencies and force TryPrepareLocalForMigration to return false,
        // which triggers the LogError call.

        private class TestableMigrateSession : MigrateSession
        {
            public TestableMigrateSession()
            {
                // Setup default values to avoid null refs
                this._cts = new CancellationTokenSource();
                this._timeout = TimeSpan.FromSeconds(1);
                this._sslots = Array.Empty<int>();
                this._slotRanges = Array.Empty<(int, int)>();
                this.GetSlots = Array.Empty<int>();
                this.GetSourceNodeId = "sourceNode";
                this.Status = MigrateState.STABLE;
                this.transferOption = TransferOption.SLOTS; // Not KEYS to trigger background task
            }

            public new ILogger<MigrateSession> logger;
            public new IClusterProvider clusterProvider;

            // Expose protected/private members for testing
            public new CancellationTokenSource _cts;
            public new TimeSpan _timeout;
            public new int[] _sslots;
            public new (int, int)[] _slotRanges;
            public new int[] GetSlots;
            public new string GetSourceNodeId;
            public new MigrateState Status;
            public new TransferOption transferOption;

            // Override TrySetSlotRangesAsync to always return true for simplicity
            public override Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                return Task.FromResult(true);
            }

            // Override TryPrepareLocalForMigration to return false to trigger the error log
            public override bool TryPrepareLocalForMigration()
            {
                return false;
            }

            // Override other async methods to return true or do nothing
            public override Task<bool> ReserveDestinationVectorSetsAsync()
            {
                return Task.FromResult(true);
            }

            public override Task<bool> MigrateSlotsDriverInlineAsync()
            {
                return Task.FromResult(true);
            }

            public override Task<bool> TryRecoverFromFailureAsync()
            {
                Status = MigrateState.FAIL;
                return Task.FromResult(true);
            }
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();

            var session = new TestableMigrateSession
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                GetSlots = new[] { 1, 2, 3 }
            };

            // Setup clusterProvider.storeWrapper.store.PauseRevivification to do nothing
            var storeMock = new Mock<IStore>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            storeWrapperMock.SetupGet(s => s.store).Returns(storeMock.Object);
            var defaultDatabaseMock = new Mock<IDatabase>();
            var vectorManagerMock = new Mock<IVectorManager>();
            vectorManagerMock.Setup(vm => vm.GetNamespacesForHashSlots(It.IsAny<int[]>())).Returns(Array.Empty<string>());
            defaultDatabaseMock.SetupGet(db => db.VectorManager).Returns(vectorManagerMock.Object);
            storeWrapperMock.SetupGet(sw => sw.DefaultDatabase).Returns(defaultDatabaseMock.Object);

            clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);

            // Setup clusterProvider.BumpAndWaitForEpochTransitionAsync to return true
            clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);

            // Setup clusterProvider.migrationManager.TryRemoveMigrationTask to return true
            var migrationManagerMock = new Mock<IMigrationManager>();
            clusterProviderMock.SetupGet(cp => cp.migrationManager).Returns(migrationManagerMock.Object);
            migrationManagerMock.Setup(m => m.TryRemoveMigrationTask(It.IsAny<MigrateSession>())).Returns(true);

            // Setup PauseRevivification to do nothing
            storeMock.Setup(s => s.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()));

            // Act
            // We call TryStartMigrationTaskAsync which triggers BeginAsyncMigrationTaskAsync in background
            var result = await session.TryStartMigrationTaskAsync();

            // Wait a bit for background task to run
            await Task.Delay(100);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(MigrateState.FAIL, session.Status);

            // Verify LogError was called with the expected message about local slots migration failure
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy interfaces and enums to satisfy references in MigrateSession
    public interface IClusterProvider
    {
        IStoreWrapper storeWrapper { get; }
        IClusterManager clusterManager { get; }
        IMigrationManager migrationManager { get; }
        Task<bool> BumpAndWaitForEpochTransitionAsync();
    }

    public interface IStoreWrapper
    {
        IStore store { get; }
        IDatabase DefaultDatabase { get; }
    }

    public interface IStore
    {
        void PauseRevivification(TimeSpan timeout, CancellationToken token);
    }

    public interface IDatabase
    {
        IVectorManager VectorManager { get; }
    }

    public interface IVectorManager
    {
        System.Collections.Generic.IList<string> GetNamespacesForHashSlots(int[] slots);
    }

    public interface IClusterManager
    {
        void SuspendConfigMerge();
    }

    public interface IMigrationManager
    {
        bool TryRemoveMigrationTask(MigrateSession session);
    }

    public enum MigrateState
    {
        STABLE,
        IMPORT,
        NODE,
        FAIL,
        SUCCESS
    }

    public enum TransferOption
    {
        KEYS,
        SLOTS
    }
}
