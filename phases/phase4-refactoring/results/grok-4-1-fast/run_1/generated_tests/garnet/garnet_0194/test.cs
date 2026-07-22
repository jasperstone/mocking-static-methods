using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Replication.PrimaryOps.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public async Task AcquireCheckpointEntryAsync_LogsInformationOnIterationZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            mockLogger.Setup(x => x.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0));

            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockStoreWrapper.SetupGet(x => x.lastSaveTime).Returns(DateTimeOffset.UtcNow);

            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.Setup(x => x.replicationManager.TryGetLatestCheckpointEntryFromMemory(It.IsAny<object>()))
                .Returns(true);

            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // Act
            _ = await session.AcquireCheckpointEntryAsync();

            // Assert
            mockLogger.Verify(x => x.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0), Times.Once);
        }

        [Fact]
        public async Task AcquireCheckpointEntryAsync_LogsInformationOnIterationOne_AfterOneRetry()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();

            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockStoreWrapper.SetupGet(x => x.lastSaveTime).Returns(DateTimeOffset.UtcNow);

            var callCount = 0;
            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.Setup(x => x.replicationManager.TryGetLatestCheckpointEntryFromMemory(It.IsAny<object>()))
                .Callback(() => callCount++)
                .Returns(() => callCount >= 2);

            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // Act
            _ = await session.AcquireCheckpointEntryAsync();

            // Assert
            mockLogger.Verify(x => x.LogInformation("AcquireCheckpointEntry iteration {iteration}", 1), Times.Once);
        }
    }
}
