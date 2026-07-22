using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace MigrationTests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceAndReturnsTrue_OnSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clientMock = new Mock<IRedisClient>();
            var migrationManagerMock = new Mock<IMigrationManager>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var vectorManagerMock = new Mock<IVectorManager>();
            var defaultDatabaseMock = new Mock<IDatabase>();
            var storeMock = new Mock<IStore>();
            var clusterMock = new Mock<IClusterProvider>();

            var session = new MigrateSession
            {
                logger = loggerMock.Object,
                Status = MigrateState.UNKNOWN,
                _timeout = TimeSpan.FromSeconds(10),
                _cts = new System.Threading.CancellationTokenSource(),
                migrateOperation = new[] { new MigrationOperation { Client = clientMock.Object } },
                _sslots = new[] { 1, 2, 3 },
                _slotRanges = new[] { 1, 2, 3 },
                clusterProvider = clusterMock.Object
            };

            // Setup client mock to return "OK"
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<int[]>()))
                .ReturnsAsync("OK");

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            Assert.True(result);
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
