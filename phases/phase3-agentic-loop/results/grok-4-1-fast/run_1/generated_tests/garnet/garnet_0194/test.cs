using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Server.Replication.PrimaryOps.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public void AcquireCheckpointEntryAsync_LogsInformationOnFirstIteration()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();

            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);

            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // First iteration logs before TryGetLatestCheckpointEntryFromMemory call
            mockReplicationManager.Setup(rm => rm.TryGetLatestCheckpointEntryFromMemory(It.IsAny<CheckpointEntry>()))
                .Returns(true);

            // Act
            _ = session.AcquireCheckpointEntryAsync().GetAwaiter().GetResult();

            // Assert - logs iteration 0 on first loop
            mockLogger.Verify(
                x => x.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0),
                Times.Once);
        }

        [Fact]
        public void AcquireCheckpointEntryAsync_LogsInformationOnRetryIteration()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();

            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);

            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // First call fails (logs iteration 0, retries), second succeeds (logs iteration 1)
            mockReplicationManager.SetupSequence(rm => rm.TryGetLatestCheckpointEntryFromMemory(It.IsAny<CheckpointEntry>()))
                .Returns(false)
                .Returns(true);

            // Act
            _ = session.AcquireCheckpointEntryAsync().GetAwaiter().GetResult();

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0),
                Times.Once);

            mockLogger.Verify(
                x => x.LogInformation("AcquireCheckpointEntry iteration {iteration}", 1),
                Times.Once);
        }
    }
}
