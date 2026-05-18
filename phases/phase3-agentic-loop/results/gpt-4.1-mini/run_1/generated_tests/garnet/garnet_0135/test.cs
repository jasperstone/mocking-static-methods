using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.test.cluster
{
    public class MigrateSessionTests
    {
        private class TestableMigrateSession : MigrateSession
        {
            public Mock<ILogger> LoggerMock { get; }
            public Mock<IClusterProvider> ClusterProviderMock { get; }

            public bool MigrateSlotsDriverInlineResult { get; set; } = true;

            public TestableMigrateSession()
                : base(
                    clusterSession: null,
                    clusterProvider: null,
                    _targetAddress: "127.0.0.1",
                    _targetPort: 6379,
                    _targetNodeId: "targetNodeId",
                    _username: null,
                    _passwd: null,
                    _sourceNodeId: "sourceNodeId",
                    _copyOption: false,
                    _replaceOption: false,
                    _timeout: 1000,
                    _slots: new HashSet<int> { 1, 2, 3 },
                    sketch: null,
                    transferOption: TransferOption.SLOTS)
            {
                LoggerMock = new Mock<ILogger>();
                var loggerField = typeof(MigrateSession).GetField("logger", BindingFlags.Instance | BindingFlags.NonPublic);
                loggerField.SetValue(this, LoggerMock.Object);

                ClusterProviderMock = new Mock<IClusterProvider>();
                var clusterProviderField = typeof(MigrateSession).GetField("clusterProvider", BindingFlags.Instance | BindingFlags.NonPublic);
                clusterProviderField.SetValue(this, ClusterProviderMock.Object);

                SetupDefaults();
            }

            private void SetupDefaults()
            {
                var storeMock = new Mock<IStore>();
                storeMock.Setup(s => s.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<System.Threading.CancellationToken>()));
                storeMock.Setup(s => s.ResumeRevivification());

                var storeWrapperMock = new Mock<IStoreWrapper>();
                storeWrapperMock.SetupGet(s => s.store).Returns(storeMock.Object);

                var vectorManagerMock = new Mock<IVectorManager>();
                vectorManagerMock.Setup(v => v.GetNamespacesForHashSlots(It.IsAny<int[]>())).Returns(new List<ulong>());

                var databaseMock = new Mock<IDatabase>();
                databaseMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);

                storeWrapperMock.SetupGet(s => s.DefaultDatabase).Returns(databaseMock.Object);

                ClusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);

                var clusterManagerMock = new Mock<IClusterManager>();
                clusterManagerMock.Setup(c => c.SuspendConfigMerge());
                clusterManagerMock.Setup(c => c.ResumeConfigMerge());
                clusterManagerMock.Setup(c => c.TryMeetAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

                ClusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);

                var migrationManagerMock = new Mock<IMigrationManager>();
                migrationManagerMock.Setup(m => m.TryRemoveMigrationTask(It.IsAny<MigrateSession>())).Returns(true);

                ClusterProviderMock.SetupGet(c => c.migrationManager).Returns(migrationManagerMock.Object);

                ClusterProviderMock.Setup(c => c.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            }

            public override Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                return Task.FromResult(true);
            }

            public override bool TryPrepareLocalForMigration()
            {
                return true;
            }

            public override Task<bool> ReserveDestinationVectorSetsAsync()
            {
                return Task.FromResult(true);
            }

            public override Task<bool> MigrateSlotsDriverInlineAsync()
            {
                return Task.FromResult(MigrateSlotsDriverInlineResult);
            }
        }

        // Minimal interfaces for mocking
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
            void PauseRevivification(TimeSpan timeout, System.Threading.CancellationToken token);
            void ResumeRevivification();
        }

        public interface IDatabase
        {
            IVectorManager VectorManager { get; }
        }

        public interface IVectorManager
        {
            List<ulong> GetNamespacesForHashSlots(int[] slots);
        }

        public interface IClusterManager
        {
            void SuspendConfigMerge();
            void ResumeConfigMerge();
            Task TryMeetAsync(string address, int port, bool acquireLock);
        }

        public interface IMigrationManager
        {
            bool TryRemoveMigrationTask(MigrateSession session);
        }

        [Fact]
        public async Task BeginAsyncMigrationTask_LogsErrorOnMigrateSlotsDriverFailure()
        {
            var session = new TestableMigrateSession();

            session.MigrateSlotsDriverInlineResult = false;

            var method = typeof(MigrateSession).GetMethod("BeginAsyncMigrationTaskAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            var task = (Task)method.Invoke(session, null);
            await task;

            session.LoggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("MigrateSlotsDriver failed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
