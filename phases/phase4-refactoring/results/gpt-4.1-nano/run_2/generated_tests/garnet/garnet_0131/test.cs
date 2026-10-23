using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_ShouldLogError_When_TrySetSlotRangesAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<IStore>();
            var mockClient = new Mock<IClient>();
            var mockVectorManager = new Mock<VectorManager>();

            // Setup dependencies
            var session = new TestMigrateSession
            {
                Logger = loggerMock.Object,
                ClusterProvider = new ClusterProvider
                {
                    storeWrapper = new StoreWrapper
                    {
                        store = mockStore.Object,
                        DefaultDatabase = new Database { VectorManager = mockVectorManager.Object }
                    },
                    clusterManager = new ClusterManager(),
                    migrationManager = new MigrationManager()
                },
                Timeout = TimeSpan.FromSeconds(10),
                Cts = new CancellationTokenSource(),
                Status = MigrateState.INIT,
                GetSourceNodeId = "node1",
                GetSlots = new int[] { 1, 2, 3 },
                _sslots = new int[] { 1, 2, 3 }
            };

            // Mock TrySetSlotRangesAsync to return false to trigger LogError
            var sessionMock = new Mock<TestMigrateSession>();
            sessionMock.Setup(s => s.TrySetSlotRangesAsync(It.IsAny<string>(), It.IsAny<MigrateState>()))
                .ReturnsAsync(false);

            // Act
            await sessionMock.Object.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }

    // Helper subclass to expose internal members for testing
    internal class TestMigrateSession : MigrateSession
    {
        public ILogger Logger { get; set; }
        public ClusterProvider ClusterProvider { get; set; }
        public TimeSpan Timeout { get; set; }
        public CancellationTokenSource Cts { get; set; }
        public MigrateState Status { get; set; }
        public string GetSourceNodeId { get; set; }
        public int[] GetSlots { get; set; }
        public int[] _sslots { get; set; }

        public override async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
        {
            // Simulate failure for test
            return false;
        }

        public async Task BeginAsyncMigrationTaskAsync()
        {
            await base.BeginAsyncMigrationTaskAsync();
        }
    }
}
