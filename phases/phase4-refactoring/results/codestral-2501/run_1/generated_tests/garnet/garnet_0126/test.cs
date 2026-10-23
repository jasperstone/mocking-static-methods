using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsErrorOnTimeout()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<GarnetClientSession>();
            mockClient.Setup(client => client.SetSlotRange(It.IsAny<Memory<byte>>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ThrowsAsync(new OperationCanceledException());

            var migrateSession = new MigrateSession(mockLogger.Object, mockClient.Object);

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }

    internal sealed partial class MigrateSession : IDisposable
    {
        private readonly ILogger<MigrateSession> logger;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string[] _sslots = new string[] { "slot1", "slot2" };
        private readonly string[] _slotRanges = new string[] { "range1", "range2" };
        private readonly MigrateOperation[] migrateOperation = new MigrateOperation[] { new MigrateOperation() };

        public MigrateSession(ILogger<MigrateSession> logger, GarnetClientSession client)
        {
            this.logger = logger;
            migrateOperation[0].Client = client;
        }

        public async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
        {
            var client = migrateOperation[0].Client as GarnetClientSession;
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

                logger?.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", ClusterManager.GetRange([.. _sslots]));

                var result = await client.SetSlotRange(stateBytes, nodeid, _slotRanges)
                    .WaitAsync(_timeout, _cts.Token).ConfigureAwait(false);

                // Check if setslotsrange executed correctly
                if (!result.Equals("OK", StringComparison.Ordinal))
                {
                    logger?.LogError("SetSlotRange error: {error}", result);
                    Status = MigrateState.FAIL;
                    return false;
                }

                logger?.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", ClusterManager.GetRange([.. _sslots]), state, nodeid ?? "");
                return true;
            }
            catch (OperationCanceledException)
            {
                logger?.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", _timeout.TotalMilliseconds, ClusterManager.GetRange([.. _sslots]));
                Status = MigrateState.FAIL;
                return false;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred during SetSlotRange for slots {slots}", ClusterManager.GetRange([.. _sslots]));
                Status = MigrateState.FAIL;
                return false;
            }
        }

        private Task<bool> CheckConnectionAsync(GarnetClientSession client)
        {
            return Task.FromResult(true);
        }

        private MigrateState Status { get; set; }

        private static class ClusterManager
        {
            public static string GetRange(string[] slots)
            {
                return string.Join(",", slots);
            }
        }

        private class MigrateOperation
        {
            public object Client { get; set; }
        }

        private const string IMPORTING = "IMPORTING";
        private const string STABLE = "STABLE";
        private const string NODE = "NODE";

        public void Dispose()
        {
            // Dispose logic here
        }
    }

    internal enum MigrateState
    {
        IMPORT,
        STABLE,
        NODE,
        FAIL,
        SUCCESS
    }

    internal class GarnetClientSession
    {
        public Task<string> SetSlotRange(Memory<byte> stateBytes, string nodeid, string[] slotRanges)
        {
            return Task.FromResult("OK");
        }
    }
}
