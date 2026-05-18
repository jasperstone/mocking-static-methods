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
        // Helper to create a MigrateSession with mocked dependencies and logger
        private MigrateSession CreateMigrateSessionWithLogger(out Mock<ILogger> loggerMock, out Mock<ILoggerFactory> loggerFactoryMock)
        {
            loggerMock = new Mock<ILogger>();
            loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(p => p.loggerFactory).Returns(loggerFactoryMock.Object);

            // Setup minimal required constructor parameters
            var slots = new HashSet<int> { 1, 2, 3 };
            var clusterSession = null as ClusterSession;
            var sketch = null as Sketch;

            // We use TransferOption.SLOTS to trigger BeginAsyncMigrationTaskAsync path
            var migrateSession = new MigrateSession(
                clusterSession,
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
                sketch,
                TransferOption.SLOTS);

            return migrateSession;
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var migrateSession = CreateMigrateSessionWithLogger(out var loggerMock, out var loggerFactoryMock);

            // Setup clusterProvider and TrySetSlotRangesAsync to fail
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(p => p.loggerFactory).Returns(loggerFactoryMock.Object);
            clusterProviderMock.SetupGet(p => p.storeWrapper).Returns(new StoreWrapperMock());
            clusterProviderMock.SetupGet(p => p.clusterManager).Returns(new ClusterManagerMock());
            clusterProviderMock.SetupGet(p => p.migrationManager).Returns(new MigrationManagerMock());

            // Replace clusterProvider with mock
            var clusterProviderField = typeof(MigrateSession).GetField("clusterProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clusterProviderField.SetValue(migrateSession, clusterProviderMock.Object);

            // Setup TrySetSlotRangesAsync to return false to trigger LogError on line 154
            var migrateSessionMock = new Mock<MigrateSession>(
                null, clusterProviderMock.Object, "127.0.0.1", 6379, "targetNodeId", "user", "pass", "sourceNodeId",
                false, false, 1000, new HashSet<int> { 1, 2, 3 }, null, TransferOption.SLOTS) { CallBase = true };

            migrateSessionMock.Setup(m => m.TrySetSlotRangesAsync(It.IsAny<string>(), MigrateState.IMPORT))
                .ReturnsAsync(false);

            // Act
            var beginAsyncMigrationTaskAsyncMethod = typeof(MigrateSession).GetMethod("BeginAsyncMigrationTaskAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)beginAsyncMigrationTaskAsyncMethod.Invoke(migrateSessionMock.Object, null);
            await task;

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

        // Mocks for dependencies to allow compilation and minimal behavior
        private class StoreWrapperMock
        {
            public StoreMock store = new StoreMock();
            public DefaultDatabaseMock DefaultDatabase = new DefaultDatabaseMock();
        }

        private class StoreMock
        {
            public void PauseRevivification(TimeSpan timeout, System.Threading.CancellationToken token) { }
        }

        private class DefaultDatabaseMock
        {
            public VectorManagerMock VectorManager = new VectorManagerMock();
        }

        private class VectorManagerMock
        {
            public HashSet<ulong> GetNamespacesForHashSlots(HashSet<int> slots) => new HashSet<ulong>();
        }

        private class ClusterManagerMock
        {
            public void SuspendConfigMerge() { }
        }

        private class MigrationManagerMock
        {
            public NetworkBufferSettings GetNetworkBufferSettings => null;
            public LimitedFixedBufferPool GetNetworkPool => null;
            public bool TryRemoveMigrationTask(MigrateSession session) => true;
        }
    }
}
