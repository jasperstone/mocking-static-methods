using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    // Internal derived class to expose TrySetSlotRangesAsync for testing
    internal class TestableMigrateSession : MigrateSession
    {
        public TestableMigrateSession(
            ClusterSession clusterSession,
            ClusterProvider clusterProvider,
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
            : base(clusterSession, clusterProvider, targetAddress, targetPort, targetNodeId, username, passwd, sourceNodeId, copyOption, replaceOption, timeout, slots, sketch, transferOption)
        {
        }

        public Task<bool> CallTrySetSlotRangesAsync(string nodeid, MigrateState state)
            => TrySetSlotRangesAsync(nodeid, state);
    }

    public class MigrateSessionTests
    {
        private TestableMigrateSession CreateSessionWithMockedClient(
            Func<Task<string>> setSlotRangeResultFunc,
            out Mock<ILogger> loggerMock)
        {
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterSessionMock = new Mock<ClusterSession>();

            loggerMock = new Mock<ILogger>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            clusterProviderMock.SetupGet(p => p.loggerFactory).Returns(loggerFactoryMock.Object);

            var migrationManagerMock = new Mock<IMigrationManager>();
            migrationManagerMock.SetupGet(m => m.GetNetworkBufferSettings).Returns(new NetworkBufferSettings());
            migrationManagerMock.SetupGet(m => m.GetNetworkPool).Returns(new LimitedFixedBufferPool());
            clusterProviderMock.SetupGet(p => p.migrationManager).Returns(migrationManagerMock.Object);

            var serverOptionsMock = new Mock<IServerOptions>();
            serverOptionsMock.SetupGet(o => o.ParallelMigrateTaskCount).Returns(1);
            clusterProviderMock.SetupGet(p => p.serverOptions).Returns(serverOptionsMock.Object);

            var storeWrapperMock = new Mock<IStoreWrapper>();
            clusterProviderMock.SetupGet(p => p.storeWrapper).Returns(storeWrapperMock.Object);

            var clusterManagerMock = new Mock<IClusterManager>();
            clusterProviderMock.SetupGet(p => p.clusterManager).Returns(clusterManagerMock.Object);

            var session = (TestableMigrateSession)Activator.CreateInstance(
                typeof(TestableMigrateSession),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[]
                {
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
                    new HashSet<int> { 1, 2, 3 },
                    null,
                    TransferOption.SLOTS
                },
                null);

            // Use reflection to get migrateOperation array and set Client property on first element
            var migrateOperationField = typeof(MigrateSession).GetField("migrateOperation", BindingFlags.NonPublic | BindingFlags.Instance);
            var migrateOperations = (Array)migrateOperationField.GetValue(session);
            var migrateOperation0 = migrateOperations.GetValue(0);

            var clientMock = new Mock<GarnetClientSession>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<Memory<byte>>(), It.IsAny<string>(), It.IsAny<List<(int, int)>>()))
                .Returns(() => setSlotRangeResultFunc());
            clientMock.SetupGet(c => c.IsConnected).Returns(true);
            clientMock.Setup(c => c.ReconnectAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
            clientMock.Setup(c => c.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult("OK"));

            var gcsField = migrateOperation0.GetType().GetField("gcs", BindingFlags.NonPublic | BindingFlags.Instance);
            gcsField.SetValue(migrateOperation0, clientMock.Object);

            return session;
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceOnSendingAndCompleted()
        {
            // Arrange
            var session = CreateSessionWithMockedClient(
                setSlotRangeResultFunc: () => Task.FromResult("OK"),
                out var loggerMock);

            // Act
            var result = await session.CallTrySetSlotRangesAsync("nodeid", MigrateState.STABLE);

            // Assert
            Assert.True(result);

            // Verify LogTrace was called with "Sending CLUSTER SETSLOTRANGE"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending CLUSTER SETSLOTRANGE")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify LogTrace was called with "[Completed] SETSLOT"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Completed] SETSLOT")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
