using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace MigrationDriverTests
{
    public class MigrationDriverTest
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_TrySetSlotRangesAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var storeMock = new Mock<IStore>();
            var clientMock = new Mock<IRedisClient>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var migrationManagerMock = new Mock<IMigrationManager>();
            var vectorManagerMock = new Mock<IVectorManager>();

            // Setup dependencies
            var session = new MigrateSession
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                Status = MigrateState.INIT,
                _timeout = TimeSpan.FromSeconds(10),
                _cts = new CancellationTokenSource(),
                _sslots = new int[] { 1, 2, 3 },
                GetSourceNodeId = "node1",
                GetSlots = new int[] { 1, 2, 3 },
                _namespaces = null,
                transferOption = TransferOption.SLOTS,
            };

            // Setup clusterProvider mock
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(sw => sw.store).Returns(storeMock.Object);
            storeMock.Setup(s => s.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()));

            // Setup TrySetSlotRangesAsync to return false to trigger LogError
            var sessionMock = new Mock<MigrateSession>();
            sessionMock.CallBase = true;
            sessionMock.Setup(s => s.TrySetSlotRangesAsync(It.IsAny<string>(), It.IsAny<MigrateState>()))
                .ReturnsAsync(false);

            // Act
            await sessionMock.Object.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
