using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        private class DummyClient
        {
            public Func<string, string, byte[][], Task<string>> SetSlotRangeAsync { get; set; }
            public Task<string> SetSlotRange(byte[] stateBytes, string nodeid, byte[][] slotRanges)
            {
                return SetSlotRangeAsync?.Invoke(stateBytes.ToString(), nodeid, slotRanges) ?? Task.FromResult("OK");
            }
        }

        private class DummyClusterManager
        {
            public static byte[][] GetRange(byte[][] slots) => slots;
        }

        private class DummyStoreWrapper
        {
            public DummyVectorManager VectorManager { get; } = new DummyVectorManager();
        }

        private class DummyVectorManager
        {
            public System.Collections.Generic.List<string> GetNamespacesForHashSlots(int[] slots) => new System.Collections.Generic.List<string> { "ns" };
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper { get; } = new DummyStoreWrapper();
            public DummyClusterManager clusterManager { get; } = new DummyClusterManager();
            public Task<bool> BumpAndWaitForEpochTransitionAsync() => Task.FromResult(true);
            public Task<bool> ReserveDestinationVectorSetsAsync() => Task.FromResult(true);
        }

        private enum DummyMigrateState { FAIL, SUCCESS, IMPORT, STABLE, NODE }

        [Fact]
        public async Task TrySetSlotRangesAsync_Should_LogError_When_ResultIsNotOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new DummyClient
            {
                SetSlotRangeAsync = (stateBytes, nodeid, slotRanges) => Task.FromResult("ErrorResult")
            };
            var session = new MigrateSession
            {
                logger = loggerMock.Object,
                migrateOperation = new[] { new { Client = clientMock } },
                _timeout = TimeSpan.FromSeconds(1),
                _cts = new CancellationTokenSource(),
                Status = MigrateState.STABLE,
                _sslots = new int[] { 1, 2, 3 },
                _slotRanges = new byte[0],
                ClusterManager = new DummyClusterManager(),
                CheckConnectionAsync = (client) => Task.FromResult(true),
                ResetLocalSlot = () => { },
                clusterProvider = new DummyClusterProvider()
            };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
