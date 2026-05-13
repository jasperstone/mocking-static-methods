using System;
using System.Collections.Generic;
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
        // Helper to create a MigrateSession with mocked dependencies
        private MigrateSession CreateSessionWithLogger(out Mock<ILogger> loggerMock, bool setFailOnCheckConnection = false, string setSlotRangeResult = "OK", bool throwOnSetSlotRange = false, bool throwOperationCanceled = false)
        {
            // Mock logger
            loggerMock = new Mock<ILogger>();

            // We need to mock ClusterProvider and other dependencies minimally
            var clusterProviderMock = new Mock<ClusterProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            clusterProviderMock.SetupGet(p => p.loggerFactory).Returns(loggerFactoryMock.Object);

            // Setup minimal serverOptions for ParallelMigrateTaskCount
            var serverOptionsMock = new Mock<ServerOptions>();
            serverOptionsMock.SetupGet(o => o.ParallelMigrateTaskCount).Returns(1);
            clusterProviderMock.SetupGet(p => p.serverOptions).Returns(serverOptionsMock.Object);

            // Setup migrationManager to avoid null refs
            var migrationManagerMock = new Mock<MigrationManager>();
            clusterProviderMock.SetupGet(p => p.migrationManager).Returns(migrationManagerMock.Object);

            // Setup clusterManager to avoid null refs
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterProviderMock.SetupGet(p => p.clusterManager).Returns(clusterManagerMock.Object);

            // Setup storeWrapper to avoid null refs
            var storeWrapperMock = new Mock<StoreWrapper>();
            clusterProviderMock.SetupGet(p => p.storeWrapper).Returns(storeWrapperMock.Object);

            // Setup a dummy ClusterSession
            var clusterSessionMock = new Mock<ClusterSession>();

            // Setup slots for migration
            var slots = new HashSet<int> { 1, 2, 3 };

            // Create the MigrateSession instance
            var session = new MigrateSession(
                clusterSessionMock.Object,
                clusterProviderMock.Object,
                "127.0.0.1",
                6379,
                "targetNodeId",
                "user",
                "pass",
                "sourceNodeId",
                copyOption: false,
                replaceOption: false,
                timeout: 1000,
                slots,
                sketch: null,
                TransferOption.SLOTS);

            // We need to mock the migrateOperation[0].Client to simulate SetSlotRange behavior
            var clientMock = new Mock<GarnetClientSession>();

            // Setup CheckConnectionAsync to return true or false based on setFailOnCheckConnection
            var checkConnectionMethod = typeof(MigrateSession).GetMethod("CheckConnectionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            checkConnectionMethod.Invoke(session, new object[] { clientMock.Object });

            // Setup client.SetSlotRange to simulate different results or exceptions
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<Memory<byte>>(), It.IsAny<string>(), It.IsAny<List<(int, int)>>()))
                .Returns(() =>
                {
                    if (throwOperationCanceled)
                    {
                        var tcs = new TaskCompletionSource<string>();
                        tcs.SetException(new OperationCanceledException());
                        return tcs.Task;
                    }
                    if (throwOnSetSlotRange)
                    {
                        var tcs = new TaskCompletionSource<string>();
                        tcs.SetException(new Exception("SetSlotRange failure"));
                        return tcs.Task;
                    }
                    return Task.FromResult(setSlotRangeResult);
                });

            // Inject the mocked client into migrateOperation[0]
            var migrateOperationField = typeof(MigrateSession).GetField("migrateOperation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var migrateOperations = (MigrateOperation[])migrateOperationField.GetValue(session);
            var migrateOperation = migrateOperations[0];
            var clientField = typeof(MigrateOperation).GetField("Client", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            clientField.SetValue(migrateOperation, clientMock.Object);

            // Setup CheckConnectionAsync to return false if requested
            if (setFailOnCheckConnection)
            {
                var checkConnectionAsync = typeof(MigrateSession).GetMethod("CheckConnectionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                // Replace CheckConnectionAsync with a delegate returning false
                // This is complicated, so instead we will mock client.IsConnected to false and client.ReconnectAsync to fail
                clientMock.Setup(c => c.IsConnected).Returns(false);
                clientMock.Setup(c => c.ReconnectAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
                clientMock.Setup(c => c.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult("ERR"));
            }

            return session;
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ReturnsFalse_WhenCheckConnectionFails_LogsError()
        {
            var session = CreateSessionWithLogger(out var loggerMock, setFailOnCheckConnection: true);

            var result = await session.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Migrate CheckConnection Authentication Error")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ReturnsFalse_AndLogsError_WhenSetSlotRangeReturnsNotOk()
        {
            var session = CreateSessionWithLogger(out var loggerMock, setSlotRangeResult: "FAIL");

            var result = await session.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ReturnsFalse_AndLogsError_WhenSetSlotRangeThrowsOperationCanceledException()
        {
            var session = CreateSessionWithLogger(out var loggerMock, throwOperationCanceled: true);

            var result = await session.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange operation timed out or was cancelled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ReturnsFalse_AndLogsError_WhenSetSlotRangeThrowsException()
        {
            var session = CreateSessionWithLogger(out var loggerMock, throwOnSetSlotRange: true);

            var result = await session.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during SetSlotRange")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
