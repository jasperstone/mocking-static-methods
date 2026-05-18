using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        // Minimal mocks and stubs for dependencies
        private class DummyClient : IDisposable
        {
            public Func<Task<string>> SetSlotRangeFunc { get; set; } = () => Task.FromResult("OK");
            public Task<string> SetSlotRange(byte[] stateBytes, string nodeid, List<(int, int)> slotRanges)
            {
                return SetSlotRangeFunc();
            }
            public void Dispose() { }
        }

        private class DummyMigrateOperation : IDisposable
        {
            public DummyClient Client { get; }
            public DummyMigrateOperation(DummyClient client) => Client = client;
            public void Dispose() { }
        }

        private class TestMigrateSession : MigrateSession
        {
            public ILogger Logger { get; }
            public DummyClient DummyClient { get; }
            public DummyMigrateOperation DummyOperation { get; }
            public List<(int, int)> SlotRanges { get; }

            public TestMigrateSession(ILogger logger, Func<Task<bool>> checkConnectionAsync, Func<Task<string>> setSlotRangeAsync, TimeSpan timeout)
                : base(
                    clusterSession: null,
                    clusterProvider: CreateClusterProvider(logger),
                    _targetAddress: "127.0.0.1",
                    _targetPort: 6379,
                    _targetNodeId: "targetNode",
                    _username: null,
                    _passwd: null,
                    _sourceNodeId: "sourceNode",
                    _copyOption: false,
                    _replaceOption: false,
                    _timeout: (int)timeout.TotalMilliseconds,
                    _slots: new HashSet<int> { 1, 2, 3 },
                    sketch: null,
                    transferOption: TransferOption.SLOTS)
            {
                Logger = logger;
                DummyClient = new DummyClient { SetSlotRangeFunc = setSlotRangeAsync };
                DummyOperation = new DummyMigrateOperation(DummyClient);
                SlotRanges = new List<(int, int)> { (0, 1) };

                // Override migrateOperation with dummy
                var field = typeof(MigrateSession).GetField("migrateOperation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, new[] { DummyOperation });

                // Override CheckConnectionAsync to use provided delegate
                _checkConnectionAsync = checkConnectionAsync;
            }

            private readonly Func<Task<bool>> _checkConnectionAsync;

            protected override Task<bool> CheckConnectionAsync(GarnetClientSession client)
            {
                return _checkConnectionAsync();
            }
        }

        private static ClusterProvider CreateClusterProvider(ILogger logger)
        {
            var mockProvider = new Mock<ClusterProvider>(MockBehavior.Loose, null, null, null, null);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger);
            mockProvider.SetupGet(p => p.loggerFactory).Returns(mockLoggerFactory.Object);
            return mockProvider.Object;
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenSetSlotRangeReturnsError()
        {
            var loggerMock = new Mock<ILogger>();
            var session = new TestMigrateSession(
                loggerMock.Object,
                () => Task.FromResult(true),
                () => Task.FromResult("ERROR"),
                TimeSpan.FromSeconds(1));

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenOperationCanceledExceptionThrown()
        {
            var loggerMock = new Mock<ILogger>();
            var session = new TestMigrateSession(
                loggerMock.Object,
                () => Task.FromResult(true),
                () => throw new OperationCanceledException(),
                TimeSpan.FromMilliseconds(500));

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange operation timed out or was cancelled")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenExceptionThrown()
        {
            var loggerMock = new Mock<ILogger>();
            var session = new TestMigrateSession(
                loggerMock.Object,
                () => Task.FromResult(true),
                () => throw new Exception("fail"),
                TimeSpan.FromSeconds(1));

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during SetSlotRange")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
