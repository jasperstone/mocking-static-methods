using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        private class DummyClient : GarnetClientSession
        {
            public bool IsConnected { get; set; } = true;
            public string Response { get; set; } = "OK";
            public override Task<string> ReconnectAsync(int timeoutMs) => Task.FromResult(Response);
            public override Task<string> Authenticate(string username, string password) => Task.FromResult(Response);
            public override Task<string> SetSlotRange(ReadOnlyMemory<byte> stateBytes, string nodeId, List<(int, int)> ranges)
            {
                return Task.FromResult(Response);
            }
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorAndFails_WhenSetSlotRangeReturnsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mockClient = new DummyClient();
            var mockMigrationManager = new Mock<MigrationManager>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStore = new Mock<Store>();
            var mockVectorManager = new Mock<VectorManager>();

            mockClusterProvider.Setup(cp => cp.migrationManager).Returns(mockMigrationManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(sw => sw.DefaultDatabase.VectorManager).Returns(mockVectorManager.Object);
            mockClusterProvider.Setup(cp => cp.loggerFactory).Returns(new LoggerFactory());

            var session = new MigrateSession(
                new ClusterSession(),
                mockClusterProvider.Object,
                "127.0.0.1",
                6379,
                "nodeId",
                "user",
                "pass",
                "sourceNode",
                false,
                false,
                1000,
                new HashSet<int> { 1, 2, 3 },
                null,
                TransferOption.SLOTS);

            // Inject the dummy client
            session.GetType().GetField("migrateOperation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(session, new[] { new MigrateOperation(session) });
            var migrateOp = (MigrateOperation)session.GetType().GetField("migrateOperation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(session);
            migrateOp.Client = mockClient;

            // Force the client to return error
            mockClient.Response = "Error";

            // Act
            var result = await session.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            loggerMock.VerifyLog(log => log.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }

    public static class LoggerExtensions
    {
        public static void VerifyLog(this Mock<ILogger> loggerMock, Action<ILogger> logExpression, Times times)
        {
            loggerMock.Verify(log => log.Log(It.Is<LogEntry>(entry => logExpression(entry.Log)), times));
        }
    }

    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public EventId EventId { get; set; }
        public object State { get; set; }
        public Exception Exception { get; set; }
        public Func<object, Exception, string> Formatter { get; set; }
        public string Log => Formatter(State, Exception);
    }
}
