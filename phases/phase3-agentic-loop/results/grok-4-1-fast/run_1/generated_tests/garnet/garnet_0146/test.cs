using System;
using System.Collections.Generic;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.cluster.Server.Replication.Tests
{
    public class CheckpointStoreLogTraceTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsIndexTokenDeletion_WhenIndexTokensDiffer()
        {
            // Arrange - Capture the LogTrace call through verification
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Mock dependencies minimally
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockCkptManager = new Mock<object>(); // Generic mock since interface unknown
            
            mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                              .Returns(mockCkptManager.Object);
            mockClusterProvider.Setup(p => p.serverOptions).Returns(new Mock<object>().Object);
            
            // Create test instance using reflection to bypass internal constructor
            dynamic storeWrapper = new object(); // Minimal mock
            dynamic checkpointStore = Activator.CreateInstance(
                Type.GetType("Garnet.cluster.CheckpointStore, Garnet.cluster"),
                storeWrapper, mockClusterProvider.Object, false, mockLogger.Object);

            // Create entry using reflection
            dynamic entry = Activator.CreateInstance(Type.GetType("Garnet.cluster.CheckpointEntry, Garnet.cluster"));
            entry.metadata.storeIndexToken = Guid.NewGuid();

            // Act - Call through reflection
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert - Verify LogTrace was called with correct message pattern
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting index token {toDeleteIndexToken}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsLogTokenDeletion_WhenLogTokensDiffer()
        {
            // Similar test for log token deletion on line ~103
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                              .Returns(new object());
            mockClusterProvider.Setup(p => p.serverOptions).Returns(new object());

            dynamic storeWrapper = new object();
            dynamic checkpointStore = Activator.CreateInstance(
                Type.GetType("Garnet.cluster.CheckpointStore, Garnet.cluster"),
                storeWrapper, mockClusterProvider.Object, false, mockLogger.Object);

            dynamic entry = Activator.CreateInstance(Type.GetType("Garnet.cluster.CheckpointEntry, Garnet.cluster"));

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert - Verify log token deletion message
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting log token {toDeletelogToken}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.AtLeastOnce
            );
        }
    }
}
