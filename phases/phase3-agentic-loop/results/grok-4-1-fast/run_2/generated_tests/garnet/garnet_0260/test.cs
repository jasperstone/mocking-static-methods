using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_WhenCurrentStatusNotNoRecovery_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            var mockClusterProvider = new Mock<object>();
            var replicationManager = (ReplicationManager)Activator.CreateInstance(typeof(ReplicationManager), mockClusterProvider.Object, mockLogger.Object)!;

            // Use reflection to set currentRecoveryStatus to non-NoRecovery
            var currentStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            currentStatusField!.SetValue(replicationManager, RecoveryStatus.InitializeRecover);

            // Act
            var beginRecoveryMethod = typeof(ReplicationManager).GetMethod("BeginRecovery", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (bool)beginRecoveryMethod.Invoke(replicationManager, new object[] { RecoveryStatus.ReadRole, false })!;

            // Assert
            Assert.False(result);
            
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_WhenCannotPauseCheckpoints_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            var mockClusterProvider = new Mock<object>();
            // Set storeWrapper to return false for TryPauseCheckpoints
            var mockStoreWrapper = new Mock<object>();
            mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(x => x.TryPauseCheckpoints()).Returns(false);

            var replicationManager = (ReplicationManager)Activator.CreateInstance(typeof(ReplicationManager), mockClusterProvider.Object, mockLogger.Object)!;

            // Ensure currentRecoveryStatus is NoRecovery
            var currentStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            currentStatusField!.SetValue(replicationManager, RecoveryStatus.NoRecovery);

            // Act
            var beginRecoveryMethod = typeof(ReplicationManager).GetMethod("BeginRecovery", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (bool)beginRecoveryMethod.Invoke(replicationManager, new object[] { RecoveryStatus.FullRecovery, false })!;

            // Assert
            Assert.False(result);
            
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire checkpoint lock")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_WhenCannotAcquireLock_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            var mockClusterProvider = new Mock<object>();
            var mockStoreWrapper = new Mock<object>();
            mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(x => x.TryPauseCheckpoints()).Returns(true);
            mockStoreWrapper.Setup(x => x.ResumeCheckpoints());

            var replicationManager = (ReplicationManager)Activator.CreateInstance(typeof(ReplicationManager), mockClusterProvider.Object, mockLogger.Object)!;

            // Set currentRecoveryStatus to NoRecovery
            var currentStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            currentStatusField!.SetValue(replicationManager, RecoveryStatus.NoRecovery);

            // Set recoverLock to fail TryWriteLock
            var recoverLockField = typeof(ReplicationManager).GetField("recoverLock", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var mockLock = new Mock<object>();
            mockLock.Setup(x => x.TryWriteLock()).Returns(false);
            recoverLockField!.SetValue(replicationManager, mockLock.Object);

            // Act
            var beginRecoveryMethod = typeof(ReplicationManager).GetMethod("BeginRecovery", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (bool)beginRecoveryMethod.Invoke(replicationManager, new object[] { RecoveryStatus.FullRecovery, false })!;

            // Assert
            Assert.False(result);
            
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire recover lock")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
