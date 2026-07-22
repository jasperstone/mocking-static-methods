using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_TrySetSlotRangesAsync_Fails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockMigrationManager = new Mock<MigrationManager>();
            var mockClusterSession = new Mock<ClusterSession>();

            // Setup mocks
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.migrationManager).Returns(mockMigrationManager.Object);
            mockClusterProvider.Setup(cp => cp.loggerFactory).Returns(new LoggerFactory());

            var slots = new System.Collections.Generic.HashSet<int> { 1, 2, 3 };
            var session = new MigrateSession(
                mockClusterSession.Object,
                mockClusterProvider.Object,
                "127.0.0.1",
                6379,
                "targetNodeId",
                "user",
                "pass",
                "sourceNodeId",
                false,
                false,
                1000,
                slots,
                null,
                TransferOption.SLOTS);

            // Inject the mock logger
            typeof(MigrateSession).GetProperty("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(session, mockLogger.Object);

            // Act
            await session.BeginAsyncMigrationTaskAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set remote slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
