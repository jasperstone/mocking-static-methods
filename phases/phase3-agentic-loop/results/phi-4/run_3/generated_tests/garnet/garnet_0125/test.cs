using System;
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
            var migrateOperation = new[] { new MockMigrateOperation() };
            var clusterManager = new MockClusterManager();
            var migrationDriver = new MigrationDriver(loggerMock.Object, migrateOperation, clusterManager);

            // Act
            bool result = await migrationDriver.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.LogTrace(
                    "Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}",
                    MigrateState.IMPORT,
                    "nodeid",
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        private class MockMigrateOperation : IMigrateOperation
        {
            public MockClient Client { get; } = new MockClient();
        }

        private class MockClient
        {
            public Task<bool> CheckConnectionAsync() => Task.FromResult(true);

            public Task<string> SetSlotRange(byte[] stateBytes, string nodeid, object slotRanges)
            {
                return Task.FromResult("OK");
            }
        }

        private class MockClusterManager : IClusterManager
        {
            public string GetRange(object slots) => "0-16383";
        }

        private class MigrationDriver
        {
            private readonly ILogger _logger;
            private readonly IMigrateOperation[] _migrateOperation;
            private readonly IClusterManager _clusterManager;

            public MigrationDriver(ILogger logger, IMigrateOperation[] migrateOperation, IClusterManager clusterManager)
            {
                _logger = logger;
                _migrateOperation = migrateOperation;
                _clusterManager = clusterManager;
            }

            public async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                var client = _migrateOperation[0].Client;
                try
                {
                    if (!await client.CheckConnectionAsync().ConfigureAwait(false))
                    {
                        return false;
                    }

                    var stateBytes = state switch
                    {
                        MigrateState.IMPORT => new byte[] { 1 },
                        MigrateState.STABLE => new byte[] { 2 },
                        MigrateState.NODE => new byte[] { 3 },
                        _ => throw new Exception("Invalid SETSLOT Operation"),
                    };

                    _logger.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", _clusterManager.GetRange(null));

                    var result = await client.SetSlotRange(stateBytes, nodeid, null).ConfigureAwait(false);

                    if (!result.Equals("OK", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    _logger.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", _clusterManager.GetRange(null), state, nodeid ?? "");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private interface IMigrateOperation
        {
            MockClient Client { get; }
        }

        private interface IClusterManager
        {
            string GetRange(object slots);
        }

        private enum MigrateState
        {
            IMPORT,
            STABLE,
            NODE
        }
    }
}
