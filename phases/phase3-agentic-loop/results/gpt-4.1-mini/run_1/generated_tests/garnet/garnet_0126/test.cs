using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    // We cannot access internal MigrateState enum, so we define a local enum with matching values for testing.
    internal enum TestMigrateState : byte
    {
        SUCCESS = 0x0,
        FAIL,
        PENDING,
        IMPORT,
        STABLE,
        NODE,
    }

    public class MigrateSessionLoggerTests
    {
        // Dummy client interface to simulate SetSlotRange calls
        public interface IClient
        {
            Task<string> SetSlotRange(byte[] stateBytes, string nodeid, object slotRanges);
        }

        public class MigrateOperation
        {
            public IClient Client { get; set; }
        }

        // Dummy class simulating the TrySetSlotRangesAsync method behavior focusing on logger calls
        private class DummyMigrateSession
        {
            private readonly ILogger _logger;
            private readonly TimeSpan _timeout;
            private readonly CancellationTokenSource _cts;
            private readonly MigrateOperation[] _migrateOperation;
            private TestMigrateState _status;

            public DummyMigrateSession(ILogger logger, Func<Task<bool>> checkConnectionAsync, Func<Task<string>> setSlotRangeAsync)
            {
                _logger = logger;
                _timeout = TimeSpan.FromMilliseconds(100);
                _cts = new CancellationTokenSource();

                var mockClient = new Mock<IClient>();
                mockClient.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                    .Returns(() => setSlotRangeAsync());

                _migrateOperation = new[] { new MigrateOperation { Client = mockClient.Object } };

                CheckConnectionAsync = checkConnectionAsync;
            }

            public TestMigrateState Status
            {
                get => _status;
                set => _status = value;
            }

            public Func<Task<bool>> CheckConnectionAsync { get; }

            public async Task<bool> TrySetSlotRangesAsync(string nodeid, TestMigrateState state)
            {
                var client = _migrateOperation[0].Client;
                try
                {
                    if (!await CheckConnectionAsync().ConfigureAwait(false))
                    {
                        Status = TestMigrateState.FAIL;
                        return false;
                    }

                    var stateBytes = state switch
                    {
                        TestMigrateState.IMPORT => new byte[] { 1 },
                        TestMigrateState.STABLE => new byte[] { 2 },
                        TestMigrateState.NODE => new byte[] { 3 },
                        _ => throw new Exception("Invalid SETSLOT Operation"),
                    };

                    // Simulate LogTrace call omitted

                    var result = await client.SetSlotRange(stateBytes, nodeid, null)
                        .WaitAsync(_timeout, _cts.Token).ConfigureAwait(false);

                    if (!result.Equals("OK", StringComparison.Ordinal))
                    {
                        _logger?.LogError("SetSlotRange error: {error}", result);
                        Status = TestMigrateState.FAIL;
                        return false;
                    }

                    // Simulate LogTrace call omitted
                    return true;
                }
                catch (OperationCanceledException)
                {
                    _logger?.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", _timeout.TotalMilliseconds, null);
                    Status = TestMigrateState.FAIL;
                    return false;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "An error occurred during SetSlotRange for slots {slots}", null);
                    Status = TestMigrateState.FAIL;
                    return false;
                }
            }
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenSetSlotRangeReturnsError()
        {
            var loggerMock = new Mock<ILogger>();
            var session = new DummyMigrateSession(
                loggerMock.Object,
                () => Task.FromResult(true),
                () => Task.FromResult("ERROR"));

            var result = await session.TrySetSlotRangesAsync("node1", TestMigrateState.IMPORT);

            Assert.False(result);
            Assert.Equal(TestMigrateState.FAIL, session.Status);
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
            var session = new DummyMigrateSession(
                loggerMock.Object,
                () => Task.FromResult(true),
                () => throw new OperationCanceledException());

            var result = await session.TrySetSlotRangesAsync("node1", TestMigrateState.IMPORT);

            Assert.False(result);
            Assert.Equal(TestMigrateState.FAIL, session.Status);
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
            var session = new DummyMigrateSession(
                loggerMock.Object,
                () => Task.FromResult(true),
                () => throw new Exception("fail"));

            var result = await session.TrySetSlotRangesAsync("node1", TestMigrateState.IMPORT);

            Assert.False(result);
            Assert.Equal(TestMigrateState.FAIL, session.Status);
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
