using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        private class DummyClient
        {
            public virtual Task<string> SetSlotRange(byte[] stateBytes, string nodeId, int[] slots)
            {
                return Task.FromResult("OK");
            }
        }

        private class DummyClusterManager
        {
            public static int[] GetRange(int[] slots) => slots;
        }

        private class DummyStoreWrapper
        {
            public DummyVectorManager VectorManager { get; } = new DummyVectorManager();
        }

        private class DummyVectorManager
        {
            public System.Collections.Generic.List<string> GetNamespacesForHashSlots(int[] slots) => new System.Collections.Generic.List<string>();
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public DummyClusterManager clusterManager = new DummyClusterManager();

            public Task<bool> BumpAndWaitForEpochTransitionAsync() => Task.FromResult(true);
        }

        private class DummyMigrateOperation
        {
            public DummyClient Client { get; } = new DummyClient();
            public void Dispose() { }
        }

        private class DummyLogger : ILogger<MigrateSession>
        {
            public readonly System.Collections.Generic.List<string> Logs = new();

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Logs.Add(formatter(state, exception));
            }
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_Should_LogError_When_ResultIsNotOK()
        {
            // Arrange
            var logger = new DummyLogger();
            var session = new MigrateSession(
                clusterSession: null,
                clusterProvider: new DummyClusterProvider(),
                _targetAddress: "127.0.0.1",
                _targetPort: 6379,
                _targetNodeId: "node1",
                _username: null,
                _passwd: null,
                _sourceNodeId: "source1",
                _copyOption: false,
                _replaceOption: false,
                _timeout: 1000,
                _slots: new System.Collections.Generic.HashSet<int> { 1, 2, 3 },
                sketch: null,
                transferOption: TransferOption.SLOTS
            );
            session.logger = logger;
            session.migrateOperation = new[] { new DummyMigrateOperation() };
            var clientMock = new Mock<DummyClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int[]>()))
                      .ReturnsAsync("ERROR");
            session.migrateOperation[0] = new DummyMigrateOperation { Client = clientMock.Object };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            Assert.False(result);
            Assert.Contains("SetSlotRange error:", string.Join(Environment.NewLine, logger.Logs));
        }
    }
}
