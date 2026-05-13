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
                this._checkConnectionResult = checkConnectionResult;
                this._setSlotRangeResult = setSlotRangeResult;
            }

            public ILogger logger;
            public MigrateOperation[] migrateOperation;
            public int[] _sslots;
            public object _slotRanges;
            public TimeSpan _timeout;
            public CancellationTokenSource _cts;
            public MigrateState Status;

            private readonly bool _checkConnectionResult;
            private readonly string _setSlotRangeResult;

            public override async Task<bool> CheckConnectionAsync(DummyClient client)
            {
                await Task.Yield();
                return _checkConnectionResult;
            }

            public override async Task<string> SetSlotRangeAsync(DummyClient client, byte[] stateBytes, string nodeid, object slotRanges)
            {
                await Task.Yield();
                return _setSlotRangeResult;
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

        // We need to mock or override these members to test TrySetSlotRangesAsync
        private abstract class MigrateSession
        {
            public abstract Task<bool> CheckConnectionAsync(DummyClient client);
            public abstract Task<string> SetSlotRangeAsync(DummyClient client, byte[] stateBytes, string nodeid, object slotRanges);

            public ILogger logger;
            public MigrateOperation[] migrateOperation;
            public int[] _sslots;
            public object _slotRanges;
            public TimeSpan _timeout;
            public CancellationTokenSource _cts;
            public MigrateState Status;

            protected static readonly byte[] IMPORTING = new byte[] { 1 };
            protected static readonly byte[] STABLE = new byte[] { 2 };
            protected static readonly byte[] NODE = new byte[] { 3 };

            public async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
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

                    var result = await SetSlotRangeAsync(client, stateBytes, nodeid, _slotRanges)
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

            public static string GetRange(int[] slots)
            {
                return string.Join(",", slots);
            }
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceOnStartAndCompletion()
        {
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<DummyClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("OK");

            var session = new TestMigrateSession(loggerMock.Object, clientMock.Object);

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            Assert.True(result);
            Assert.Equal(MigrateState.SUCCESS, session.Status);

            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending CLUSTER SETSLOTRANGE")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Completed] SETSLOT")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorAndFails_WhenSetSlotRangeReturnsError()
        {
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<DummyClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("ERROR");

            var session = new TestMigrateSession(loggerMock.Object, clientMock.Object, true, "ERROR");

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, session.Status);

            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorAndFails_WhenCheckConnectionFails()
        {
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<DummyClient>();

            var session = new TestMigrateSession(loggerMock.Object, clientMock.Object, false);

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, session.Status);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorAndFails_OnOperationCanceledException()
        {
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<DummyClient>();

            var session = new TestMigrateSession(loggerMock.Object, clientMock.Object);

            // Override SetSlotRangeAsync to throw OperationCanceledException
            session.SetSlotRangeAsync = (client, stateBytes, nodeid, slotRanges) =>
            {
                throw new OperationCanceledException();
            };

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, session.Status);

            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange operation timed out or was cancelled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorAndFails_OnGeneralException()
        {
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<DummyClient>();

            var session = new TestMigrateSession(loggerMock.Object, clientMock.Object);

            // Override SetSlotRangeAsync to throw general exception
            session.SetSlotRangeAsync = (client, stateBytes, nodeid, slotRanges) =>
            {
                throw new Exception("Test exception");
            };

            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, session.Status);

            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during SetSlotRange")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
