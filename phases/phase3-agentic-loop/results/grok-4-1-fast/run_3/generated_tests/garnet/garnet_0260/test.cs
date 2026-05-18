using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerLoggerTests
    {
        [Fact]
        public void BeginRecovery_CurrentRecoveryStatusNotNoRecovery_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<object>(); // Minimal mock for storeWrapper
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);

            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            
            // Set currentRecoveryStatus via reflection (field is public but class is internal)
            var currentStatusField = typeof(ReplicationManager)
                .GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentStatusField?.SetValue(replicationManager, RecoveryStatus.InitializeRecover);

            // Act
            var result = (bool)typeof(ReplicationManager)
                .GetMethod("BeginRecovery", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(replicationManager, new object[] { RecoveryStatus.ClusterReplicate, false });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((object state) => 
                        state?.ToString()?.Contains("Error background recovering task has not completed") == true &&
                        state?.ToString()?.Contains("ClusterReplicate") == true),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_TryPauseCheckpointsFails_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<object>();
            mockStoreWrapper.Setup(x => x.Equals(It.IsAny<object>())).Returns(true); // Minimal setup
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(x => x.GetType()).Returns(typeof(object)); // Avoid issues

            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            
            var currentStatusField = typeof(ReplicationManager)
                .GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentStatusField?.SetValue(replicationManager, RecoveryStatus.NoRecovery);

            // Mock TryPauseCheckpoints via reflection or property setup
            mockStoreWrapper.Setup(x => ((dynamic)x).TryPauseCheckpoints()).Returns(false);

            // Act
            var result = (bool)typeof(ReplicationManager)
                .GetMethod("BeginRecovery", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(replicationManager, new object[] { RecoveryStatus.ClusterReplicate, false });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((object state) => 
                        state?.ToString()?.Contains("Error could not acquire checkpoint lock") == true &&
                        state?.ToString()?.Contains("ClusterReplicate") == true),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void BeginRecovery_TryLockFails_LogsError()
        {
            // Arrange - Test the recover lock failure path
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<object>();
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            
            mockStoreWrapper.Setup(x => x.GetType()).Returns(typeof(object));

            var replicationManager = new ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            
            var currentStatusField = typeof(ReplicationManager)
                .GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentStatusField?.SetValue(replicationManager, RecoveryStatus.NoRecovery);

            mockStoreWrapper.Setup(x => ((dynamic)x).TryPauseCheckpoints()).Returns(true);

            // Act
            var result = (bool)typeof(ReplicationManager)
                .GetMethod("BeginRecovery", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(replicationManager, new object[] { RecoveryStatus.ClusterReplicate, false });

            // Assert - Lock failure logs the specific message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((object state) => 
                        state?.ToString()?.Contains("Error could not acquire recover lock") == true),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            Assert.False(result);
        }
    }
}
