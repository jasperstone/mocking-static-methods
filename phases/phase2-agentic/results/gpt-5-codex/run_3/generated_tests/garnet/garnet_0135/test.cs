using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Cluster.Server.Migration
{
    public class MigrationDriverTests
    {
        private readonly Mock<ILogger> _mockLogger = new(MockBehavior.Strict);

        private MigrateSession CreateSession(bool relinquishOwnershipResult, bool migrateDriverResult = true)
        {
            var clusterProvider = new Mock<IClusterProvider>();
            var clusterManager = new Mock<IClusterManager>();
            var storeWrapper = new Mock<IStoreWrapper>();
            var store = new Mock<IStore>();
            var migrationManager = new Mock<IMigrationManager>();
            var defaultDatabase = new Mock<IDatabase>();
            var vectorManager = new Mock<IVectorManager>();

            clusterProvider.SetupGet(cp => cp.clusterManager).Returns(clusterManager.Object);
            clusterProvider.SetupGet(cp => cp.storeWrapper).Returns(storeWrapper.Object);
            clusterProvider.SetupGet(cp => cp.migrationManager).Returns(migrationManager.Object);

            storeWrapper.SetupGet(sw => sw.store).Returns(store.Object);
            storeWrapper.SetupGet(sw => sw.DefaultDatabase).Returns(defaultDatabase.Object);

            defaultDatabase.SetupGet(dd => dd.VectorManager).Returns(vectorManager.Object);

            store.Setup(s => s.ResumeRevivification());
            store.Setup(s => s.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()));

            var session = new TestableMigrateSession(
                clusterProvider.Object,
                _mockLogger.Object,
                migrateDriverResult,
                relinquishOwnershipResult);

            return session;
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_WhenRelinquishOwnershipFails_LogsError()
        {
            var session = CreateSession(relinquishOwnershipResult: false);
            _mockLogger.Setup(l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to relinquish ownership from source node")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            await session.InvokeBeginAsyncMigrationTaskAsync();

            _mockLogger.Verify();
        }

        private sealed class TestableMigrateSession : MigrateSession
        {
            private readonly bool _migrateDriverResult;
            private readonly bool _relinquishOwnershipResult;

            public TestableMigrateSession(
                IClusterProvider clusterProvider,
                ILogger logger,
                bool migrateDriverResult,
                bool relinquishOwnershipResult)
                : base(clusterProvider)
            {
                this.logger = logger;
                _migrateDriverResult = migrateDriverResult;
                _relinquishOwnershipResult = relinquishOwnershipResult;
            }

            protected override Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state) => Task.FromResult(true);

            protected override Task<bool> MigrateSlotsDriverInlineAsync() => Task.FromResult(_migrateDriverResult);

            protected override bool RelinquishOwnership() => _relinquishOwnershipResult;

            protected override Task<bool> ReserveDestinationVectorSetsAsync() => Task.FromResult(true);

            protected override bool TryPrepareLocalForMigration() => true;

            protected override void ResetLocalSlot()
            {
            }

            public Task InvokeBeginAsyncMigrationTaskAsync() => BeginAsyncMigrationTaskAsync();
        }
    }
}
