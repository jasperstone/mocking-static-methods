using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class MigrateSessionTests
    {
        // We cannot instantiate MigrateSession directly because it is internal.
        // Instead, we will test the logging behavior by mocking the dependencies and invoking the BeginAsyncMigrationTaskAsync method via reflection.

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.SetupGet(p => p.loggerFactory).Returns(loggerFactoryMock.Object);

            // Setup minimal required properties and methods on clusterProviderMock to avoid null refs
            clusterProviderMock.SetupGet(p => p.serverOptions).Returns(new ServerOptionsStub());
            clusterProviderMock.SetupGet(p => p.migrationManager).Returns(new MigrationManagerStub());
            clusterProviderMock.SetupGet(p => p.storeWrapper).Returns(new StoreWrapperStub());
            clusterProviderMock.SetupGet(p => p.clusterManager).Returns(new ClusterManagerStub());

            var clusterSessionMock = new Mock<IClusterSession>();

            var slots = new HashSet<int> { 1, 2, 3 };

            // We cannot instantiate MigrateSession directly because it is internal.
            // Instead, we create a derived test class inside the same namespace to access internal members.
            var testSession = new TestMigrateSession(
                clusterSessionMock.Object,
                clusterProviderMock.Object,
                "127.0.0.1",
                6379,
                "targetNodeId",
                "user",
                "pass",
                "sourceNodeId",
                false,
                false,
                1000,
                slots,
                null,
                TransferOption.SLOTS);

            // Act
            await testSession.InvokeBeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set remote slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Derived class inside the test namespace to access internal members and override TrySetSlotRangesAsync
        internal class TestMigrateSession : MigrateSession
        {
            public TestMigrateSession(
                IClusterSession clusterSession,
                IClusterProvider clusterProvider,
                string targetAddress,
                int targetPort,
                string targetNodeId,
                string username,
                string passwd,
                string sourceNodeId,
                bool copyOption,
                bool replaceOption,
                int timeout,
                HashSet<int> slots,
                object sketch,
                TransferOption transferOption)
                : base(
                    clusterSession,
                    clusterProvider,
                    targetAddress,
                    targetPort,
                    targetNodeId,
                    username,
                    passwd,
                    sourceNodeId,
                    copyOption,
                    replaceOption,
                    timeout,
                    slots,
                    sketch,
                    transferOption)
            {
            }

            public async Task InvokeBeginAsyncMigrationTaskAsync()
            {
                await BeginAsyncMigrationTaskAsync();
            }

            // Override TrySetSlotRangesAsync to simulate failure on first call (import state)
            protected override Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                if (state == MigrateState.IMPORT)
                {
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            }
        }

        // Stubs and interfaces to satisfy dependencies

        internal interface IClusterProvider
        {
            ILoggerFactory loggerFactory { get; }
            ServerOptionsStub serverOptions { get; }
            MigrationManagerStub migrationManager { get; }
            StoreWrapperStub storeWrapper { get; }
            ClusterManagerStub clusterManager { get; }
        }

        internal interface IClusterSession { }

        internal class ServerOptionsStub
        {
            public int ParallelMigrateTaskCount => 1;
        }

        internal class MigrationManagerStub
        {
            public NetworkBufferSettingsStub GetNetworkBufferSettings => null;
            public LimitedFixedBufferPoolStub GetNetworkPool => null;
            public bool TryRemoveMigrationTask(MigrateSession session) => true;
        }

        internal class StoreWrapperStub
        {
            public StoreStub store => new StoreStub();
            public DefaultDatabaseStub DefaultDatabase => new DefaultDatabaseStub();
        }

        internal class StoreStub
        {
            public void PauseRevivification(TimeSpan timeout, System.Threading.CancellationToken token) { }
        }

        internal class DefaultDatabaseStub
        {
            public VectorManagerStub VectorManager => new VectorManagerStub();
        }

        internal class VectorManagerStub
        {
            public HashSet<ulong> GetNamespacesForHashSlots(HashSet<int> slots) => new HashSet<ulong>();
        }

        internal class ClusterManagerStub
        {
            public void SuspendConfigMerge() { }
        }

        internal class NetworkBufferSettingsStub { }
        internal class LimitedFixedBufferPoolStub { }
    }
}
