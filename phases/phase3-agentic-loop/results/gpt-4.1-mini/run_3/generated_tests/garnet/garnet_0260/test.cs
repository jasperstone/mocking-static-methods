using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        private class MockClusterProvider : ClusterProvider
        {
            public MockClusterProvider(ClusterManager clusterManager, StoreWrapper storeWrapper, GarnetServerOptions serverOptions)
                : base(storeWrapper)
            {
                // Use reflection to set internal readonly fields
                var type = typeof(ClusterProvider);
                var clusterManagerField = type.GetField("clusterManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var serverOptionsField = type.GetField("serverOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                clusterManagerField.SetValue(this, clusterManager);
                serverOptionsField.SetValue(this, serverOptions);
            }
        }

        private class MockClusterManager : ClusterManager
        {
            public ClusterConfig CurrentConfigMock { get; set; }
            public override ClusterConfig CurrentConfig => CurrentConfigMock;
            public MockClusterManager() : base(null) { }
        }

        private class MockStoreWrapper : StoreWrapper
        {
            public Func<bool> TryPauseCheckpointsFunc = () => true;
            public Action ResumeCheckpointsAction = () => { };

            public override bool TryPauseCheckpoints() => TryPauseCheckpointsFunc();
            public override void ResumeCheckpoints() => ResumeCheckpointsAction();
        }

        private class MockServerOptions : GarnetServerOptions
        {
            public bool RecoverMock { get; set; }
            public override bool Recover => RecoverMock;
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusNotNoRecovery()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new MockStoreWrapper();
            var serverOptionsMock = new MockServerOptions { RecoverMock = false };
            var clusterManagerMock = new MockClusterManager
            {
                CurrentConfigMock = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA }
            };
            var clusterProvider = new MockClusterProvider(clusterManagerMock, storeWrapperMock, serverOptionsMock);

            var manager = new ReplicationManager(clusterProvider, loggerMock.Object);

            // Set currentRecoveryStatus to something other than NoRecovery
            var currentRecoveryStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            currentRecoveryStatusField.SetValue(manager, RecoveryStatus.ReadRole);

            var result = manager.BeginRecovery(RecoveryStatus.CheckpointRecoveredAtReplica, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenTryPauseCheckpointsFails()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new MockStoreWrapper
            {
                TryPauseCheckpointsFunc = () => false
            };
            var serverOptionsMock = new MockServerOptions { RecoverMock = false };
            var clusterManagerMock = new MockClusterManager
            {
                CurrentConfigMock = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA }
            };
            var clusterProvider = new MockClusterProvider(clusterManagerMock, storeWrapperMock, serverOptionsMock);

            var manager = new ReplicationManager(clusterProvider, loggerMock.Object);

            // Set currentRecoveryStatus to NoRecovery
            var currentRecoveryStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            currentRecoveryStatusField.SetValue(manager, RecoveryStatus.NoRecovery);

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire checkpoint lock")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_LogsErrorAndResumesCheckpoints_WhenTryLockFails()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new MockStoreWrapper
            {
                TryPauseCheckpointsFunc = () => true,
                ResumeCheckpointsAction = () => { resumed = true; }
            };
            var resumed = false;
            var serverOptionsMock = new MockServerOptions { RecoverMock = false };
            var clusterManagerMock = new MockClusterManager
            {
                CurrentConfigMock = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA }
            };
            var clusterProvider = new MockClusterProvider(clusterManagerMock, storeWrapperMock, serverOptionsMock);

            var manager = new ReplicationManager(clusterProvider, loggerMock.Object);

            // Set currentRecoveryStatus to NoRecovery
            var currentRecoveryStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            currentRecoveryStatusField.SetValue(manager, RecoveryStatus.NoRecovery);

            // Setup recoverLock to fail TryReadLock and TryWriteLock
            var recoverLockField = typeof(ReplicationManager).GetField("recoverLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var recoverLockMock = new Mock<SingleWriterMultiReaderLock>();
            recoverLockMock.Setup(r => r.TryReadLock()).Returns(false);
            recoverLockMock.Setup(r => r.TryWriteLock()).Returns(false);
            recoverLockField.SetValue(manager, recoverLockMock.Object);

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.False(result);
            Assert.True(resumed);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire recover lock")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
