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
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceOnSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IClient>();
            var migrateOperation = new[] { new MigrateOperation { Client = clientMock.Object } };
            var clusterManagerMock = new Mock<IClusterManager>();
            var migrationDriver = new MigrationDriver(
                loggerMock.Object,
                migrateOperation,
                clusterManagerMock.Object,
                TimeSpan.FromMilliseconds(100),
                new CancellationTokenSource());

            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<object[]>()))
                      .ReturnsAsync("OK");

            // Act
            var result = await migrationDriver.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                l => l.LogTrace(
                    "Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}",
                    MigrateState.IMPORT,
                    "nodeid",
                    It.IsAny<string[]>()),
                Times.Once);
        }
    }

    // Mocks and supporting classes
    public class MigrationDriver
    {
        private readonly ILogger _logger;
        private readonly MigrateOperation[] _migrateOperation;
        private readonly IClusterManager _clusterManager;
        private readonly TimeSpan _timeout;
        private readonly CancellationTokenSource _cts;

        public MigrationDriver(ILogger logger, MigrateOperation[] migrateOperation, IClusterManager clusterManager, TimeSpan timeout, CancellationTokenSource cts)
        {
            _logger = logger;
            _migrateOperation = migrateOperation;
            _clusterManager = clusterManager;
            _timeout = timeout;
            _cts = cts;
        }

        public async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
        {
            var client = _migrateOperation[0].Client;
            try
            {
                if (!await CheckConnectionAsync(client).ConfigureAwait(false))
                {
                    return false;
                }

                var stateBytes = state switch
                {
                    MigrateState.IMPORT => (byte)0x01,
                    MigrateState.STABLE => (byte)0x02,
                    MigrateState.NODE => (byte)0x03,
                    _ => throw new Exception("Invalid SETSLOT Operation"),
                };

                _logger?.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", new string[] { "slot1", "slot2" });

                var result = await client.SetSlotRange(stateBytes, nodeid, new object[] { }).ConfigureAwait(false);

                if (!result.Equals("OK", StringComparison.Ordinal))
                {
                    return false;
                }

                _logger?.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", new string[] { "slot1", "slot2" }, state, nodeid ?? "");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Task<bool> CheckConnectionAsync(IClient client)
        {
            return Task.FromResult(true);
        }
    }

    public class MigrateOperation
    {
        public IClient Client { get; set; }
    }

    public interface IClient
    {
        Task<string> SetSlotRange(byte stateBytes, string nodeid, object[] slotRanges);
    }

    public interface IClusterManager
    {
    }

    public enum MigrateState
    {
        IMPORT,
        STABLE,
        NODE
    }
}
