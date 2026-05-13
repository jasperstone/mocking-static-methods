using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        // We need to mock dependencies for ReplicationManager constructor
        // and the recoverLock to control TryUpgradeReadLock, TryReadLock, TryWriteLock behavior.
        // Also mock clusterProvider.storeWrapper.TryPauseCheckpoints and ResumeCheckpoints.

        private class DummyRecoverLock
        {
            public Func<bool> TryUpgradeReadLockFunc = () => true;
            public Func<bool> TryReadLockFunc = () => true;
            public Func<bool> TryWriteLockFunc = () => true;

            public bool TryUpgradeReadLock() => TryUpgradeReadLockFunc();
            public bool TryReadLock() => TryReadLockFunc();
            public bool TryWriteLock() => TryWriteLockFunc();
        }

        private class DummyStoreWrapper
        {
            public Func<bool> TryPauseCheckpointsFunc = () => true;
            public Action ResumeCheckpointsAction = () => { };

            public bool TryPauseCheckpoints() => TryPauseCheckpointsFunc();
            public void ResumeCheckpoints() => ResumeCheckpointsAction();
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public ClusterManager clusterManager = new ClusterManager();
            public ServerOptions serverOptions = new ServerOptions();

            public DummyClusterProvider()
            {
                clusterManager.CurrentConfig = new ClusterConfig { LocalNodeRole = NodeRole.PRIMARY };
                serverOptions.EnableAOF = true;
                serverOptions.Recover = false;
                storeWrapper.TryPauseCheckpointsFunc = () => true;
                storeWrapper.ResumeCheckpointsAction = () => { };
            }
        }

        private class ClusterManager
        {
            public ClusterConfig CurrentConfig { get; set; }
        }

        private class ClusterConfig
        {
            public NodeRole LocalNodeRole { get; set; }
        }

        private enum NodeRole
        {
            PRIMARY,
            REPLICA
        }

        private class ServerOptions
        {
            public bool EnableAOF { get; set; }
            public bool Recover { get; set; }
            public bool DisableObjects { get; set; }
            public string CheckpointDir { get; set; }
            public DeviceFactory GetInitializedDeviceFactory(string path) => new DeviceFactory();
        }

        private class DeviceFactory
        {
            public FileDescriptor Get(FileDescriptor fd) => new FileDescriptor();
        }

        private class FileDescriptor
        {
            public string directoryName;
            public string fileName;
            public long SectorSize => 512;
            public FileDescriptor(string directoryName = "", string fileName = "")
            {
                this.directoryName = directoryName;
                this.fileName = fileName;
            }
            public long GetFileSize(int offset) => 0;
        }

        // We will create a derived class to inject mocks for recoverLock and clusterProvider.storeWrapper
        private class TestReplicationManager : ReplicationManager
        {
            public DummyRecoverLock DummyRecoverLock;
            public DummyClusterProvider DummyClusterProvider;

            public TestReplicationManager(DummyClusterProvider clusterProvider, ILogger logger = null) : base(clusterProvider, logger)
            {
                DummyClusterProvider = clusterProvider;
                DummyRecoverLock = new DummyRecoverLock();
                // Override recoverLock field via reflection since it's private
                var recoverLockField = typeof(ReplicationManager).GetField("recoverLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                recoverLockField.SetValue(this, DummyRecoverLock);
            }

            public new bool BeginRecovery(RecoveryStatus nextRecoveryStatus, bool upgradeLock)
            {
                return base.BeginRecovery(nextRecoveryStatus, upgradeLock);
            }
        }

        private enum RecoveryStatus
        {
            NoRecovery,
            ReadRole,
            InitializeRecover,
            CheckpointRecoveredAtReplica
        }

        [Fact]
        public void BeginRecovery_UpgradeLock_Success_LogsTrace()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var rm = new TestReplicationManager(clusterProvider, loggerMock.Object);

            rm.DummyRecoverLock.TryUpgradeReadLockFunc = () => true;

            var result = rm.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: true);

            Assert.True(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Upgraded recover lock")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void BeginRecovery_CurrentRecoveryStatusNotNoRecovery_LogsErrorAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var rm = new TestReplicationManager(clusterProvider, loggerMock.Object);

            // Set currentRecoveryStatus to something other than NoRecovery
            var currentRecoveryStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            currentRecoveryStatusField.SetValue(rm, RecoveryStatus.ReadRole);

            var result = rm.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void BeginRecovery_TryPauseCheckpointsFails_LogsErrorAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            clusterProvider.storeWrapper.TryPauseCheckpointsFunc = () => false;
            var rm = new TestReplicationManager(clusterProvider, loggerMock.Object);

            var currentRecoveryStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            currentRecoveryStatusField.SetValue(rm, RecoveryStatus.NoRecovery);

            var result = rm.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire checkpoint lock")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void BeginRecovery_TryRecoverLockFails_LogsErrorResumesCheckpointsAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var resumed = false;
            clusterProvider.storeWrapper.ResumeCheckpointsAction = () => resumed = true;
            var rm = new TestReplicationManager(clusterProvider, loggerMock.Object);

            var currentRecoveryStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            currentRecoveryStatusField.SetValue(rm, RecoveryStatus.NoRecovery);

            clusterProvider.storeWrapper.TryPauseCheckpointsFunc = () => true;

            rm.DummyRecoverLock.TryReadLockFunc = () => false;
            rm.DummyRecoverLock.TryWriteLockFunc = () => false;

            var result = rm.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.False(result);
            Assert.True(resumed);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire recover lock")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void BeginRecovery_Success_LogsTraceAndReturnsTrue()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var rm = new TestReplicationManager(clusterProvider, loggerMock.Object);

            var currentRecoveryStatusField = typeof(ReplicationManager).GetField("currentRecoveryStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            currentRecoveryStatusField.SetValue(rm, RecoveryStatus.NoRecovery);

            clusterProvider.storeWrapper.TryPauseCheckpointsFunc = () => true;

            rm.DummyRecoverLock.TryReadLockFunc = () => true;
            rm.DummyRecoverLock.TryWriteLockFunc = () => true;

            var result = rm.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.True(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Success recover lock")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
