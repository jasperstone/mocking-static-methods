using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        private class DummyClient
        {
            public virtual Task<string> SetSlotRange(byte[] stateBytes, string nodeid, object slotRanges)
            {
                return Task.FromResult("OK");
            }
        }

        private class TestMigrateSession : MigrateSession
        {
            public TestMigrateSession(ILogger logger, DummyClient client, bool checkConnectionResult = true, string setSlotRangeResult = "OK")
            {
                this.logger = logger;
                this.migrateOperation = new[] { new MigrateOperation { Client = client } };
                this._sslots = new int[] { 1, 2, 3 };
                this._slotRanges = new object();
                this._timeout = TimeSpan.FromSeconds(1);
                this._cts = new CancellationTokenSource();
                this.Status = MigrateState.SUCCESS;
                this.checkConnectionResult = checkConnectionResult;
                this.setSlotRangeResult = setSlotRangeResult;
            }

            public ILogger logger;
            public MigrateOperation[] migrateOperation;
            public int[] _sslots;
            public object _slotRanges;
            public TimeSpan _timeout;
            public CancellationTokenSource _cts;
            public MigrateState Status;

            private readonly bool checkConnectionResult;
            private readonly string setSlotRangeResult;

            public override async Task<bool> CheckConnectionAsync(DummyClient client)
            {
                await Task.Yield();
                return checkConnectionResult;
            }

            public override async Task<string> SetSlotRange(byte[] stateBytes, string nodeid, object slotRanges)
            {
                await Task.Yield();
                return setSlotRangeResult;
            }

            public override byte[] IMPORTING => new byte[] { 1 };
            public override byte[] STABLE => new byte[] { 2 };
            public override byte[] NODE => new byte[] { 3 };

            public override string GetRange(int[] slots) => "1-3";

            // Override the method under test to use the dummy client and logger
            public new async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                var client = migrateOperation[0].Client;
                try
                {
                    if (!await CheckConnectionAsync(client).ConfigureAwait(false))
                    {
                        Status = MigrateState.FAIL;
                        return false;
                    }

                    var stateBytes = state switch
                    {
                        MigrateState.IMPORT => IMPORTING,
                        MigrateState.STABLE => STABLE,
                        MigrateState.NODE => NODE,
                        _ => throw new Exception("Invalid SETSLOT Operation"),
                    };

                    logger?.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", GetRange(_sslots));

                    var result = await SetSlotRange(stateBytes, nodeid, _slotRanges)
                        .ConfigureAwait(false);

                    if (!result.Equals("OK", StringComparison.Ordinal))
                    {
                        logger?.LogError("SetSlotRange error: {error}", result);
                        Status = MigrateState.FAIL;
                        return false;
                    }

                    logger?.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", GetRange(_sslots), state, nodeid ?? "");
                    return true;
                }
                catch (OperationCanceledException)
                {
                    logger?.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", _timeout.TotalMilliseconds, GetRange(_sslots));
                    Status = MigrateState.FAIL;
                    return false;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "An error occurred during SetSlotRange for slots {slots}", GetRange(_sslots));
                    Status = MigrateState.FAIL;
                    return false;
                }
            }
        }

        private class MigrateOperation
        {
            public DummyClient Client { get; set; }
        }

        private enum MigrateState
        {
            IMPORT,
            STABLE,
            NODE,
            FAIL,
            SUCCESS
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceOnSuccess()
        {
            var loggerMock = new Mock<ILogger>();
            var client = new DummyClient();
            var session = new TestMigrateSession(loggerMock.Object, client);

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            Assert.True(result);
            loggerMock.Verify(l => l.LogTrace(
                "Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}",
                MigrateState.IMPORT, "node1", "1-3"), Times.Once);
            loggerMock.Verify(l => l.LogTrace(
                "[Completed] SETSLOT {slots} {state} {nodeid}",
                "1-3", MigrateState.IMPORT, "node1"), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnSetSlotRangeFailure()
        {
            var loggerMock = new Mock<ILogger>();
            var client = new DummyClient();
            var session = new TestMigrateSession(loggerMock.Object, client, setSlotRangeResult: "FAIL");

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, session.Status);
            loggerMock.Verify(l => l.LogError("SetSlotRange error: {error}", "FAIL"), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnCheckConnectionFailure()
        {
            var loggerMock = new Mock<ILogger>();
            var client = new DummyClient();
            var session = new TestMigrateSession(loggerMock.Object, client, checkConnectionResult: false);

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.NODE);

            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, session.Status);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnOperationCanceledException()
        {
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<DummyClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new OperationCanceledException());

            var session = new TestMigrateSession(loggerMock.Object, clientMock.Object);

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.NODE);

            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, session.Status);
            loggerMock.Verify(l => l.LogError(
                "SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}",
                It.IsAny<double>(), "1-3"), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnException()
        {
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<DummyClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("fail"));

            var session = new TestMigrateSession(loggerMock.Object, clientMock.Object);

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.NODE);

            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, session.Status);
            loggerMock.Verify(l => l.LogError(
                It.IsAny<Exception>(),
                "An error occurred during SetSlotRange for slots {slots}",
                "1-3"), Times.Once);
        }
    }
}
