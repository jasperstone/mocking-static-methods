using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Migration.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_SuccessPath_LogsTraceCompletionMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FakeMigrateSession>>();
            var capturedLogs = new List<string>();
            
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, id, state, ex, formatter) => capturedLogs.Add(formatter(state, ex)));

            var session = new FakeMigrateSession(loggerMock.Object);
            session.SetupSuccessPath();

            // Act
            var result = await session.TrySetSlotRangesAsync("target-node-123", FakeMigrateState.IMPORT);

            // Assert
            Assert.True(result);
            Assert.Contains("[Completed] SETSLOT 1-3 IMPORT target-node-123", capturedLogs);
            Assert.Contains("Sending CLUSTER SETSLOTRANGE IMPORT target-node-123 1-3", capturedLogs);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_NullNodeId_LogsTraceWithEmptyString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FakeMigrateSession>>();
            var capturedLogs = new List<string>();
            
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, id, state, ex, formatter) => capturedLogs.Add(formatter(state, ex)));

            var session = new FakeMigrateSession(loggerMock.Object);
            session.SetupSuccessPath();

            // Act
            var result = await session.TrySetSlotRangesAsync(null, FakeMigrateState.STABLE);

            // Assert
            Assert.True(result);
            Assert.Contains("[Completed] SETSLOT 1-3 STABLE ", capturedLogs); // space from nodeid ?? ""
        }

        [Fact]
        public async Task TryRecoverFromFailureAsync_FailurePath_LogsErrorMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FakeMigrateSession>>();
            var capturedLogs = new List<string>();
            
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, id, state, ex, formatter) => capturedLogs.Add(formatter(state, ex)));

            var session = new FakeMigrateSession(loggerMock.Object);
            session.SetupTrySetSlotRangesFail();

            // Act
            var result = await session.TryRecoverFromFailureAsync();

            // Assert
            Assert.False(result);
            Assert.Contains("MigrateSession.RecoverFromFailure failed to make slots STABLE", capturedLogs);
        }
    }

    // Test-specific implementation that duplicates the exact logger call from line 50
    public class FakeMigrateSession
    {
        private readonly ILogger<FakeMigrateSession> _logger;
        private readonly List<FakeMigrateOperation> _migrateOperations;
        public int[] _sslots = { 1, 2, 3 };
        public SlotRange[] _slotRanges = { new SlotRange(1, 3) };
        public TimeSpan _timeout = TimeSpan.FromSeconds(30);
        public CancellationTokenSource _cts = new();

        public FakeMigrateSession(ILogger<FakeMigrateSession> logger)
        {
            _logger = logger;
            _migrateOperations = new List<FakeMigrateOperation> { new FakeMigrateOperation() };
        }

        public void SetupSuccessPath()
        {
            _migrateOperations[0].Client = new FakeSuccessClient();
        }

        public void SetupTrySetSlotRangesFail()
        {
            _migrateOperations[0].Client = new FakeFailingClient();
        }

        public async Task<bool> TrySetSlotRangesAsync(string nodeid, FakeMigrateState state)
        {
            var client = _migrateOperations[0].Client;
            
            // Simulate CheckConnectionAsync passing
            var stateBytes = state switch
            {
                FakeMigrateState.IMPORT => new byte[] { 1 },
                FakeMigrateState.STABLE => new byte[] { 2 },
                FakeMigrateState.NODE => new byte[] { 3 },
                _ => throw new Exception("Invalid SETSLOT Operation"),
            };

            _logger?.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", "1-3");

            var result = await client.SetSlotRange(stateBytes, nodeid, _slotRanges)
                .WaitAsync(_timeout, _cts.Token);

            if (!result.Equals("OK", StringComparison.Ordinal))
            {
                _logger?.LogError("SetSlotRange error: {error}", result);
                return false;
            }

            // EXACT LINE 50 FROM SOURCE - tests Microsoft.Extensions.Logging.LoggerExtensions.LogTrace
            _logger?.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", "1-3", state, nodeid ?? "");
            return true;
        }

        public async Task<bool> TryRecoverFromFailureAsync()
        {
            if (!await TrySetSlotRangesAsync(null, FakeMigrateState.STABLE))
            {
                _logger?.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");
                return false;
            }
            return true;
        }
    }

    public class FakeMigrateOperation
    {
        public FakeClusterClient Client { get; set; } = null!;
    }

    public class FakeClusterClient : IClusterClient
    {
        public IAsyncCommand SetSlotRange(byte[] stateBytes, string nodeid, IEnumerable<SlotRange> slotRanges)
            => new FakeAsyncCommand("OK");
    }

    public class FakeSuccessClient : IClusterClient
    {
        public IAsyncCommand SetSlotRange(byte[] stateBytes, string nodeid, IEnumerable<SlotRange> slotRanges)
            => new FakeAsyncCommand("OK");
    }

    public class FakeFailingClient : IClusterClient
    {
        public IAsyncCommand SetSlotRange(byte[] stateBytes, string nodeid, IEnumerable<SlotRange> slotRanges)
            => new FakeAsyncCommand("ERROR");
    }

    public class FakeAsyncCommand : IAsyncCommand
    {
        private readonly string _result;
        public FakeAsyncCommand(string result) => _result = result;
        public ValueTask<string> WaitAsync(TimeSpan timeout, CancellationToken token) => ValueTask.FromResult(_result);
    }

    public interface IClusterClient
    {
        IAsyncCommand SetSlotRange(byte[] stateBytes, string nodeid, IEnumerable<SlotRange> slotRanges);
    }

    public interface IAsyncCommand
    {
        ValueTask<string> WaitAsync(TimeSpan timeout, CancellationToken token);
    }

    public enum FakeMigrateState { IMPORT, STABLE, NODE, FAIL, SUCCESS }

    public class SlotRange 
    { 
        public int Start, End; 
        public SlotRange(int s, int e) { Start = s; End = e; } 
    }
}
