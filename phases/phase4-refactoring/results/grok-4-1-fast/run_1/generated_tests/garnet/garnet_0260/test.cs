using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using System.Reflection;

namespace Garnet.cluster.Server.Replication.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_WhenCurrentRecoveryStatusNotNoRecovery_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(sw => sw.TryPauseCheckpoints()).Returns(true);

            var replicationManager = new ReflectionReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            replicationManager.SetCurrentRecoveryStatus(RecoveryStatus.InitializeRecover);

            // Verify the specific LogError call (line 368)
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    t != null && t.ToString()!.Contains("Error background recovering task has not completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            mockLogger.Verify();
        }

        [Fact]
        public void BeginRecovery_WhenCannotPauseCheckpoints_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(sw => sw.TryPauseCheckpoints()).Returns(false);

            var replicationManager = new ReflectionReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            replicationManager.SetCurrentRecoveryStatus(RecoveryStatus.NoRecovery);

            // Verify checkpoint lock error
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    t != null && t.ToString()!.Contains("Error could not acquire checkpoint lock")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            mockLogger.Verify();
        }

        [Fact]
        public void BeginRecovery_WhenCannotAcquireRecoverLock_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(sw => sw.TryPauseCheckpoints()).Returns(true);
            mockStoreWrapper.Setup(sw => sw.ResumeCheckpoints()).Verifiable();

            var replicationManager = new ReflectionReplicationManager(mockClusterProvider.Object, mockLogger.Object);
            replicationManager.SetCurrentRecoveryStatus(RecoveryStatus.NoRecovery);
            replicationManager.SetRecoverLockFails(true);

            // Verify recover lock error
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    t != null && t.ToString()!.Contains("Error could not acquire recover lock")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act
            var result = replicationManager.BeginRecovery(RecoveryStatus.ReadRole, false);

            // Assert
            Assert.False(result);
            mockStoreWrapper.Verify(sw => sw.ResumeCheckpoints(), Times.Once);
            mockLogger.Verify();
        }
    }

    // Test subclass to access internal members
    public class ReflectionReplicationManager : ReplicationManager
    {
        public ReflectionReplicationManager(ClusterProvider clusterProvider, ILogger logger = null) : base(clusterProvider, logger) { }

        public void SetCurrentRecoveryStatus(RecoveryStatus status)
        {
            currentRecoveryStatus = status;
        }

        public void SetRecoverLockFails(bool fails)
        {
            recoverLock = new FailingLock();
        }

        public bool BeginRecovery(RecoveryStatus nextRecoveryStatus, bool upgradeLock) => base.BeginRecovery(nextRecoveryStatus, upgradeLock);
    }

    // Simple struct that fails lock attempts (SingleWriterMultiReaderLock is a struct)
    public struct FailingLock : SingleWriterMultiReaderLock
    {
        public bool TryReadLock() => false;
        public bool TryWriteLock() => false;
        public bool TryUpgradeReadLock() => false;
        // Minimal implementation of other required members
        public void Dispose() { }
        public void EnterReadLock() { }
        public void ExitReadLock() { }
        public void EnterWriteLock() { }
        public void ExitWriteLock() { }
    }
}
