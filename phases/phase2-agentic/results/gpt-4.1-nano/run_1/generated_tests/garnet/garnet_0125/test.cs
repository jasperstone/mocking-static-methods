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
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTrace_WhenCalled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IRedisClient>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockVectorManager = new Mock<IVectorManager>();
            var mockMigrationManager = new Mock<IMigrationManager>();
            var mockClusterProvider = new Mock<IClusterProvider>();

            var session = new MigrateSession(
                migrateOperation: new[] { new MigrateOperation { Client = mockClient.Object } },
                logger: mockLogger.Object,
                clusterProvider: mockClusterProvider.Object,
                clusterManager: mockClusterManager.Object,
                storeWrapper: mockStoreWrapper.Object,
                migrationManager: mockMigrationManager.Object,
                vectorManager: mockVectorManager.Object,
                timeout: TimeSpan.FromSeconds(10),
                cts: new CancellationTokenSource());

            // Setup
            mockClient.Setup(c => c.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<int[]>()))
                .ReturnsAsync("OK");
            mockClusterManager.Setup(c => c.GetRange(It.IsAny<int[]>()))
                .Returns(new int[] { 1, 2, 3 });
            mockClusterProvider.Setup(p => p.CheckConnectionAsync(It.IsAny<IRedisClient>()))
                .ReturnsAsync(true);

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending CLUSTER SETSLOTRANGE")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.True(result);
        }
    }
}
