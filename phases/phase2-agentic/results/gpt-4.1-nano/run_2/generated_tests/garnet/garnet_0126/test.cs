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
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public DummyClusterManager clusterManager = new DummyClusterManager();
            public Func<Task<bool>> BumpAndWaitForEpochTransitionAsync { get; set; } = () => Task.FromResult(true);
            public void PauseRevivification(TimeSpan timeout, CancellationToken token) { }
        }

        private enum DummyMigrateState { FAIL, SUCCESS, IMPORT, STABLE, NODE }

        private class DummyMigrateSession : MigrateSession
        {
            public DummyClient Client { get; set; }
            public DummyClusterProvider clusterProvider { get; set; }
            public ILogger logger { get; set; }
            public byte[][] _sslots = new byte[0][];
            public TimeSpan _timeout = TimeSpan.FromSeconds(1);
            public CancellationToken _cts = CancellationToken.None;
            public string[] _slotRanges = new string[0];
            public MigrateState Status { get; set; }
            public string GetSourceNodeId => "node1";
            public int[] GetSlots => new int[0];
            public DummyMigrateSession()
            {
                Client = new DummyClient();
                clusterProvider = new DummyClusterProvider();
                logger = new Mock<ILogger>().Object;
            }
            public override bool TryPrepareLocalForMigration() => true;
            public override Task<bool> ReserveDestinationVectorSetsAsync() => Task.FromResult(true);
            public override Task<bool> MigrateSlotsDriverInlineAsync() => Task.FromResult(true);
            public override void ResetLocalSlot() { }
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_Should_LogError_When_ResultIsNotOK()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var session = new DummyMigrateSession
            {
                logger = mockLogger.Object,
                Client = new DummyClient
                {
                    SetSlotRangeAsync = (stateBytes, nodeid, slotRanges) => Task.FromResult("ErrorResult")
                },
                Status = MigrateState.IMPORT
            };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_Should_LogError_When_OperationCanceledException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var session = new DummyMigrateSession
            {
                logger = mockLogger.Object,
                Client = new DummyClient
                {
                    SetSlotRangeAsync = (stateBytes, nodeid, slotRanges) => throw new OperationCanceledException()
                },
                _timeout = TimeSpan.FromMilliseconds(10),
                _cts = new CancellationTokenSource().Token,
                Status = MigrateState.IMPORT
            };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_Should_LogError_When_Exception()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var session = new DummyMigrateSession
            {
                logger = mockLogger.Object,
                Client = new DummyClient
                {
                    SetSlotRangeAsync = (stateBytes, nodeid, slotRanges) => throw new Exception("TestException")
                },
                Status = MigrateState.IMPORT
            };

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            mockLogger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
