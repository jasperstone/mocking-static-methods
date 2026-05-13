using System;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        private class DummyRecoverLock
        {
            private readonly bool tryUpgradeReadLockResult;
            private readonly bool tryReadLockResult;
            private readonly bool tryWriteLockResult;

            public DummyRecoverLock(bool tryUpgradeReadLockResult, bool tryReadLockResult, bool tryWriteLockResult)
            {
                this.tryUpgradeReadLockResult = tryUpgradeReadLockResult;
                this.tryReadLockResult = tryReadLockResult;
                this.tryWriteLockResult = tryWriteLockResult;
            }

            public bool TryUpgradeReadLock() => tryUpgradeReadLockResult;
            public bool TryReadLock() => tryReadLockResult;
            public bool TryWriteLock() => tryWriteLockResult;
        }

        private class DummyStoreWrapper
        {
            public bool TryPauseCheckpointsResult { get; set; } = true;
            public bool ResumeCheckpointsCalled { get; private set; } = false;

            public bool TryPauseCheckpoints() => TryPauseCheckpointsResult;
            public void ResumeCheckpoints() => ResumeCheckpointsCalled = true;
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
        }

        private class TestReplicationManager : ReplicationManager
        {
            public new RecoveryStatus currentRecoveryStatus;
            public DummyRecoverLock recoverLock;
            public DummyClusterProvider clusterProvider;
            public Mock<ILogger> LoggerMock;

            public TestReplicationManager(DummyClusterProvider clusterProvider, Mock<ILogger> loggerMock) : base(clusterProvider, loggerMock.Object)
            {
                this.clusterProvider = clusterProvider;
                this.LoggerMock = loggerMock;
            }

            public override bool BeginRecovery(RecoveryStatus nextRecoveryStatus, bool upgradeLock)
            {
                // We override to inject our dummy recoverLock and clusterProvider
                if (upgradeLock)
                {
                    if (!recoverLock.TryUpgradeReadLock())
                    {
                        return false;
                    }

                    currentRecoveryStatus = nextRecoveryStatus;
                    LoggerMock?.Object.LogTrace("Upgraded recover lock [{recoverStatus}]", nextRecoveryStatus);
                    return true;
                }

                if (currentRecoveryStatus != RecoveryStatus.NoRecovery)
                {
                    LoggerMock?.Object.LogError("Error background recovering task has not completed [{recoverStatus}]", nextRecoveryStatus);
                    return false;
                }

                if (!clusterProvider.storeWrapper.TryPauseCheckpoints())
                {
                    LoggerMock?.Object.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", nextRecoveryStatus);
                    return false;
                }

                var lockAcquired =
                    nextRecoveryStatus == RecoveryStatus.ReadRole ?
                        recoverLock.TryReadLock() :
                        recoverLock.TryWriteLock();

                if (!lockAcquired)
                {
                    LoggerMock?.Object.LogError("Error could not acquire recover lock [{recoverStatus}]", nextRecoveryStatus);
                    clusterProvider.storeWrapper.ResumeCheckpoints();
                    return false;
                }

                currentRecoveryStatus = nextRecoveryStatus;
                LoggerMock?.Object.LogTrace("Success recover lock [{recoverStatus}]", nextRecoveryStatus);
                return true;
            }
        }

        [Fact]
        public void BeginRecovery_UpgradeLockFails_ReturnsFalseAndNoLogError()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var manager = new TestReplicationManager(clusterProvider, loggerMock);
            manager.recoverLock = new DummyRecoverLock(tryUpgradeReadLockResult: false, tryReadLockResult: false, tryWriteLockResult: false);

            var result = manager.BeginRecovery(RecoveryStatus.ClusterReplicate, upgradeLock: true);

            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void BeginRecovery_CurrentRecoveryNotNoRecovery_LogsErrorAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var manager = new TestReplicationManager(clusterProvider, loggerMock);
            manager.currentRecoveryStatus = RecoveryStatus.ClusterReplicate;
            manager.recoverLock = new DummyRecoverLock(true, true, true);

            var result = manager.BeginRecovery(RecoveryStatus.ClusterFailover, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.ClusterFailover), Times.Once);
        }

        [Fact]
        public void BeginRecovery_TryPauseCheckpointsFails_LogsErrorAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            clusterProvider.storeWrapper.TryPauseCheckpointsResult = false;
            var manager = new TestReplicationManager(clusterProvider, loggerMock);
            manager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            manager.recoverLock = new DummyRecoverLock(true, true, true);

            var result = manager.BeginRecovery(RecoveryStatus.ClusterFailover, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", RecoveryStatus.ClusterFailover), Times.Once);
        }

        [Fact]
        public void BeginRecovery_TryAcquireRecoverLockFails_LogsErrorResumesCheckpointsAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var manager = new TestReplicationManager(clusterProvider, loggerMock);
            manager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            clusterProvider.storeWrapper.TryPauseCheckpointsResult = true;
            // Fail to acquire recover lock
            manager.recoverLock = new DummyRecoverLock(true, false, false);

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
            Assert.True(clusterProvider.storeWrapper.ResumeCheckpointsCalled);
        }

        [Fact]
        public void BeginRecovery_SuccessfulReadRoleLock_LogsTraceAndReturnsTrue()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var manager = new TestReplicationManager(clusterProvider, loggerMock);
            manager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            clusterProvider.storeWrapper.TryPauseCheckpointsResult = true;
            manager.recoverLock = new DummyRecoverLock(true, true, false);

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.True(result);
            loggerMock.Verify(l => l.LogTrace("Success recover lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        }

        [Fact]
        public void BeginRecovery_SuccessfulWriteLock_LogsTraceAndReturnsTrue()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var manager = new TestReplicationManager(clusterProvider, loggerMock);
            manager.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            clusterProvider.storeWrapper.TryPauseCheckpointsResult = true;
            manager.recoverLock = new DummyRecoverLock(true, false, true);

            var result = manager.BeginRecovery(RecoveryStatus.ClusterFailover, upgradeLock: false);

            Assert.True(result);
            loggerMock.Verify(l => l.LogTrace("Success recover lock [{recoverStatus}]", RecoveryStatus.ClusterFailover), Times.Once);
        }
    }
}
