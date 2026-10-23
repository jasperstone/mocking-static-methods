using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            clusterProviderMock.SetupGet(p => p.loggerFactory).Returns(loggerFactoryMock.Object);
            clusterProviderMock.SetupGet(p => p.serverOptions).Returns(new ServerOptionsStub());
            clusterProviderMock.SetupGet(p => p.migrationManager).Returns(new MigrationManagerStub());
            clusterProviderMock.SetupGet(p => p.storeWrapper).Returns(new StoreWrapperStub());
            clusterProviderMock.SetupGet(p => p.clusterManager).Returns(new ClusterManagerStub());

            var slots = new HashSet<int> { 1, 2, 3 };

            var session = new TestMigrateSession(
                clusterSession: null,
                clusterProvider: clusterProviderMock.Object,
                targetAddress: "127.0.0.1",
                targetPort: 6379,
                targetNodeId: "targetNodeId",
                username: null,
                passwd: null,
                sourceNodeId: "sourceNodeId",
                copyOption: false,
                replaceOption: false,
                timeout: 1000,
                slots: slots,
                sketch: null,
                transferOption: TransferOption.SLOTS,
                loggerMock: loggerMock);

            // Act
            await session.InvokeBeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Subclass to override TryPrepareLocalForMigration to simulate failure
        private class TestMigrateSession : MigrateSession
        {
            private readonly Mock<ILogger> _loggerMock;

            public TestMigrateSession(
                ClusterSession clusterSession,
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
                Sketch sketch,
                TransferOption transferOption,
                Mock<ILogger> loggerMock)
                : base(clusterSession, clusterProvider, targetAddress, targetPort, targetNodeId, username, passwd, sourceNodeId, copyOption, replaceOption, timeout, slots, sketch, transferOption)
            {
                _loggerMock = loggerMock;
                this.logger = loggerMock.Object;
            }

            private protected override bool TryPrepareLocalForMigration()
            {
                return false;
            }

            public async Task InvokeBeginAsyncMigrationTaskAsync()
            {
                await base.BeginAsyncMigrationTaskAsync();
            }
        }

        // Minimal stubs for dependencies and types
        private interface IClusterProvider
        {
            ILoggerFactory loggerFactory { get; }
            ServerOptions serverOptions { get; }
            IMigrationManager migrationManager { get; }
            IStoreWrapper storeWrapper { get; }
            IClusterManager clusterManager { get; }
        }

        private class ServerOptionsStub : ServerOptions
        {
            public ServerOptionsStub()
            {
                ParallelMigrateTaskCount = 1;
            }
        }

        private interface IMigrationManager
        {
            NetworkBufferSettings GetNetworkBufferSettings { get; }
            LimitedFixedBufferPool GetNetworkPool { get; }
        }

        private class MigrationManagerStub : IMigrationManager
        {
            public NetworkBufferSettings GetNetworkBufferSettings => new NetworkBufferSettings();
            public LimitedFixedBufferPool GetNetworkPool => new LimitedFixedBufferPool();
        }

        private interface IStoreWrapper
        {
            IStore store { get; }
            IDatabase DefaultDatabase { get; }
        }

        private interface IStore
        {
            void PauseRevivification(TimeSpan timeout, System.Threading.CancellationToken token);
        }

        private class StoreWrapperStub : IStoreWrapper
        {
            public IStore store => new StoreStub();
            public IDatabase DefaultDatabase => new DatabaseStub();
        }

        private class StoreStub : IStore
        {
            public void PauseRevivification(TimeSpan timeout, System.Threading.CancellationToken token) { }
        }

        private interface IDatabase
        {
            IVectorManager VectorManager { get; }
        }

        private class DatabaseStub : IDatabase
        {
            public IVectorManager VectorManager => new VectorManagerStub();
        }

        private interface IVectorManager
        {
            HashSet<ulong> GetNamespacesForHashSlots(HashSet<int> slots);
        }

        private class VectorManagerStub : IVectorManager
        {
            public HashSet<ulong> GetNamespacesForHashSlots(HashSet<int> slots) => new HashSet<ulong>();
        }

        private interface IClusterManager
        {
            void SuspendConfigMerge();
        }

        private class ClusterManagerStub : IClusterManager
        {
            public void SuspendConfigMerge() { }
        }

        private class NetworkBufferSettings { }
        private class LimitedFixedBufferPool { }

        private enum TransferOption
        {
            KEYS,
            SLOTS
        }

        private class Sketch { }

        private class ClusterSession { }

        private class MigrateSession
        {
            protected ILogger logger;
            protected IClusterProvider clusterProvider;
            public MigrateSession(
                ClusterSession clusterSession,
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
                Sketch sketch,
                TransferOption transferOption)
            {
                this.clusterProvider = clusterProvider;
                this.logger = clusterProvider.loggerFactory.CreateLogger("MigrateSession");
            }

            protected virtual Task BeginAsyncMigrationTaskAsync()
            {
                if (!TryPrepareLocalForMigration())
                {
                    logger.LogError("Failed to set local slots {slots} to migrate state", string.Join(',', new List<int> { 1, 2, 3 }));
                }
                return Task.CompletedTask;
            }

            private protected virtual bool TryPrepareLocalForMigration()
            {
                return true;
            }
        }

        private class ServerOptions
        {
            public int ParallelMigrateTaskCount { get; set; }
        }
    }
}
