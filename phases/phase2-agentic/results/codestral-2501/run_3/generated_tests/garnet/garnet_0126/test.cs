using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        private readonly Mock<ILogger<MigrateSession>> _loggerMock;
        private readonly Mock<IMigrateOperation> _migrateOperationMock;
        private readonly Mock<IClient> _clientMock;
        private readonly MigrateSession _migrateSession;

        public MigrationDriverTests()
        {
            _loggerMock = new Mock<ILogger<MigrateSession>>();
            _migrateOperationMock = new Mock<IMigrateOperation>();
            _clientMock = new Mock<IClient>();
            _migrateSession = new MigrateSession(_loggerMock.Object, _migrateOperationMock.Object, _clientMock.Object);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogErrorAndReturnFalse_WhenSetSlotRangeFails()
        {
            // Arrange
            var nodeId = "node1";
            var state = MigrateState.IMPORT;
            _clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .ReturnsAsync("ERROR");

            // Act
            var result = await _migrateSession.TrySetSlotRangesAsync(nodeId, state);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error: ERROR")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogErrorAndReturnFalse_WhenOperationIsCancelled()
        {
            // Arrange
            var nodeId = "node1";
            var state = MigrateState.IMPORT;
            _clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act
            var result = await _migrateSession.TrySetSlotRangesAsync(nodeId, state);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange operation timed out or was cancelled")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogErrorAndReturnFalse_WhenExceptionIsThrown()
        {
            // Arrange
            var nodeId = "node1";
            var state = MigrateState.IMPORT;
            _clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<SlotRange[]>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _migrateSession.TrySetSlotRangesAsync(nodeId, state);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during SetSlotRange")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }
    }

    internal interface IMigrateOperation
    {
        IClient Client { get; }
    }

    internal interface IClient
    {
        Task<string> SetSlotRange(byte stateBytes, string nodeid, SlotRange[] slotRanges);
    }

    internal class SlotRange
    {
    }

    internal sealed partial class MigrateSession : IDisposable
    {
        private readonly ILogger<MigrateSession> logger;
        private readonly IMigrateOperation migrateOperation;
        private readonly IClient client;
        private readonly TimeSpan _timeout;
        private readonly CancellationTokenSource _cts;
        private readonly SlotRange[] _slotRanges;
        private MigrateState Status;

        public MigrateSession(ILogger<MigrateSession> logger, IMigrateOperation migrateOperation, IClient client)
        {
            this.logger = logger;
            this.migrateOperation = migrateOperation;
            this.client = client;
            _timeout = TimeSpan.FromSeconds(30);
            _cts = new CancellationTokenSource();
            _slotRanges = new SlotRange[0];
        }

        public async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
        {
            try
            {
                if (!await CheckConnectionAsync(client).ConfigureAwait(false))
                {
                    Status = MigrateState.FAIL;
                    return false;
                }

                var stateBytes = state switch
                {
                    MigrateState.IMPORT => (byte)MigrateState.IMPORT,
                    MigrateState.STABLE => (byte)MigrateState.STABLE,
                    MigrateState.NODE => (byte)MigrateState.NODE,
                    _ => throw new Exception("Invalid SETSLOT Operation"),
                };

                logger?.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", ClusterManager.GetRange([.. _slotRanges]));

                var result = await client.SetSlotRange(stateBytes, nodeid, _slotRanges)
                    .WaitAsync(_timeout, _cts.Token).ConfigureAwait(false);

                // Check if setslotsrange executed correctly
                if (!result.Equals("OK", StringComparison.Ordinal))
                {
                    logger?.LogError("SetSlotRange error: {error}", result);
                    Status = MigrateState.FAIL;
                    return false;
                }

                logger?.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", ClusterManager.GetRange([.. _slotRanges]), state, nodeid ?? "");
                return true;
            }
            catch (OperationCanceledException)
            {
                logger?.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", _timeout.TotalMilliseconds, ClusterManager.GetRange([.. _slotRanges]));
                Status = MigrateState.FAIL;
                return false;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred during SetSlotRange for slots {slots}", ClusterManager.GetRange([.. _slotRanges]));
                Status = MigrateState.FAIL;
                return false;
            }
        }

        private Task<bool> CheckConnectionAsync(IClient client)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }

    internal static class ClusterManager
    {
        public static string GetRange(SlotRange[] slotRanges)
        {
            return string.Join(",", slotRanges);
        }
    }
}
