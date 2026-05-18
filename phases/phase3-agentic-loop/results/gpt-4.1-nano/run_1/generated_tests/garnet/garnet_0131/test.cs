using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        private Mock<ILogger<MigrateSession>> _loggerMock;
        private Mock<IClusterProvider> _clusterProviderMock;
        private Mock<IStoreWrapper> _storeWrapperMock;
        private Mock<IVectorManager> _vectorManagerMock;
        private Mock<IClusterManager> _clusterManagerMock;
        private MigrateSession _session;

        public MigrationDriverTests()
        {
            _loggerMock = new Mock<ILogger<MigrateSession>>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            _vectorManagerMock = new Mock<IVectorManager>();
            _clusterManagerMock = new Mock<IClusterManager>();

            _storeWrapperMock.Setup(s => s.DefaultDatabase.VectorManager).Returns(_vectorManagerMock.Object);
            _clusterProviderMock.Setup(c => c.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(c => c.clusterManager).Returns(_clusterManagerMock.Object);

            _session = new MigrateSession(
                logger: _loggerMock.Object,
                clusterProvider: _clusterProviderMock.Object,
                timeout: TimeSpan.FromSeconds(10),
                cts: new CancellationTokenSource(),
                migrateOperation: new[] { new MigrateOperation { Client = new Mock<IRedisClient>().Object } },
                getSourceNodeId: "node1",
                getSlots: new[] { 1, 2, 3 },
                sslots: new[] { 1, 2, 3 },
                transferOption: TransferOption.SLOTS);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_TrySetSlotRangesAsync_Fails()
        {
            // Arrange
            var mockSession = new Mock<MigrateSession>(
                _loggerMock.Object,
                _clusterProviderMock.Object,
                TimeSpan.FromSeconds(10),
                new CancellationTokenSource(),
                new[] { new MigrateOperation { Client = new Mock<IRedisClient>().Object } },
                "node1",
                new[] { 1, 2, 3 },
                new[] { 1, 2, 3 },
                TransferOption.SLOTS)
            { CallBase = true };

            mockSession.Setup(s => s.TrySetSlotRangesAsync(It.IsAny<string>(), It.IsAny<MigrateState>())).ReturnsAsync(false);
            mockSession.Setup(s => s.TryRecoverFromFailureAsync()).ReturnsAsync(true);
            mockSession.Setup(s => s.TryPrepareLocalForMigration()).Returns(true);
            mockSession.Setup(s => s.clusterProvider.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            mockSession.Setup(s => s.ReserveDestinationVectorSetsAsync()).ReturnsAsync(true);
            mockSession.Setup(s => s.MigrateSlotsDriverInlineAsync()).ReturnsAsync(true);

            // Act
            await mockSession.Object.BeginAsyncMigrationTaskAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.Is<string>(s => s.Contains("Failed to set remote slots")), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
