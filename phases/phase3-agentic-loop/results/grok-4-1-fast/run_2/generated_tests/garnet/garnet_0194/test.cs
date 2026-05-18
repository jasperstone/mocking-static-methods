using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.cluster.Server.Replication.PrimaryOps.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public void AcquireCheckpointEntryAsync_LogsInformationOnIterationZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0));
            
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockReplicationManager = new Mock<object>();
            mockReplicationManager.Setup(x => x.TryGetLatestCheckpointEntryFromMemory(It.IsAny<CheckpointEntry>()))
                .Returns(true);
            
            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.SetupGet(x => x.replicationManager).Returns(mockReplicationManager.Object);

            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // Act
            _ = session.AcquireCheckpointEntryAsync();

            // Assert
            mockLogger.Verify(x => x.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0), Times.Once);
        }

        [Fact]
        public void AcquireCheckpointEntryAsync_LogsWarningOnFailedLock()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.LogWarning("Could not acquire lock for existing checkpoint, retrying."));
            
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockReplicationManager = new Mock<object>();
            mockReplicationManager.Setup(x => x.TryGetLatestCheckpointEntryFromMemory(It.IsAny<CheckpointEntry>()))
                .Returns(false);
            
            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.SetupGet(x => x.replicationManager).Returns(mockReplicationManager.Object);

            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // Act
            _ = session.AcquireCheckpointEntryAsync();

            // Assert
            mockLogger.Verify(x => x.LogWarning("Could not acquire lock for existing checkpoint, retrying."), Times.Once);
        }
    }
}
