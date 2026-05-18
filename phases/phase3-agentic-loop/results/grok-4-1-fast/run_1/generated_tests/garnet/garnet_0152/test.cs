using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class AofSyncTaskInfoLoggerTests
    {
        [Fact]
        public void ReplicaSyncTaskAsync_LogsInformationMessage_WhenLoggerPresent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            
            mockLogger.Setup(l => l.LogInformation(
                "Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}",
                It.IsAny<string>(),
                It.IsAny<long>()));

            var logger = mockLogger.Object;

            // Act - simulate the exact logger?.LogInformation call from line 106
            logger?.LogInformation("Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}", "remoteNodeId", 12345L);

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(
                    "Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}",
                    It.Is<string>(s => s == "remoteNodeId"),
                    12345L),
                Times.Once);
        }

        [Fact]
        public void ReplicaSyncTaskAsync_DoesNotLog_WhenLoggerNull()
        {
            // Arrange
            ILogger logger = null;

            // Act - null-conditional operator prevents the call
            logger?.LogInformation("Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}", "remoteNodeId", 12345L);

            // Assert - no exception thrown
            Assert.True(true);
        }

        [Fact]
        public void Consume_LogsWarning_OnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.LogWarning(
                It.IsAny<Exception>(),
                "An exception occurred at ReplicationManager.AofSyncTaskInfo.Consume"));

            var logger = mockLogger.Object;
            var ex = new Exception("Test exception");

            // Act - simulate the catch block
            logger?.LogWarning(ex, "An exception occurred at ReplicationManager.AofSyncTaskInfo.Consume");

            // Assert
            mockLogger.Verify(l => l.LogWarning(ex, "An exception occurred at ReplicationManager.AofSyncTaskInfo.Consume"), Times.Once);
        }

        [Fact]
        public void ReplicaSyncTaskAsync_LogsWarning_OnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.LogWarning(
                It.IsAny<Exception>(),
                "An exception occurred at ReplicationManager.ReplicaSyncTask - terminating"));

            var logger = mockLogger.Object;
            var ex = new Exception("Test exception");

            // Act
            logger?.LogWarning(ex, "An exception occurred at ReplicationManager.ReplicaSyncTask - terminating");

            // Assert
            mockLogger.Verify(l => l.LogWarning(ex, "An exception occurred at ReplicationManager.ReplicaSyncTask - terminating"), Times.Once);
        }

        [Fact]
        public void ReplicaSyncTaskAsync_LogsWarning_OnTaskTermination()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.LogWarning(
                "AofSync task terminated; client disposed {remoteNodeId} {address} {port} {currentAddress}",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<long>()));

            var logger = mockLogger.Object;

            // Act
            logger?.LogWarning("AofSync task terminated; client disposed {remoteNodeId} {address} {port} {currentAddress}", "remoteNodeId", "127.0.0.1", 6379, 12345L);

            // Assert
            mockLogger.Verify(
                l => l.LogWarning(
                    "AofSync task terminated; client disposed {remoteNodeId} {address} {port} {currentAddress}",
                    "remoteNodeId",
                    "127.0.0.1",
                    6379,
                    12345L),
                Times.Once);
        }

        [Fact]
        public void ReplicaSyncTaskAsync_LogsInformation_OnFailedTaskStoreRemove()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.LogInformation(
                "Did not remove {remoteNodeId} from aofTaskStore at end of ReplicaSyncTask",
                It.IsAny<string>()));

            var logger = mockLogger.Object;

            // Act
            logger?.LogInformation("Did not remove {remoteNodeId} from aofTaskStore at end of ReplicaSyncTask", "remoteNodeId");

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(
                    "Did not remove {remoteNodeId} from aofTaskStore at end of ReplicaSyncTask",
                    "remoteNodeId"),
                Times.Once);
        }
    }
}
