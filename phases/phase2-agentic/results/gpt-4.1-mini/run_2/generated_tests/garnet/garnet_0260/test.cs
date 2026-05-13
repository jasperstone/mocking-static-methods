using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

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
            private readonly bool tryPauseCheckpointsResult;
            public bool ResumeCheckpointsCalled { get; private set; }

            public DummyStoreWrapper(bool tryPauseCheckpointsResult)
            {
                this.tryPauseCheckpointsResult = tryPauseCheckpointsResult;
            }

            public bool TryPauseCheckpoints() => tryPauseCheckpointsResult;
            public void ResumeCheckpoints() => ResumeCheckpointsCalled = true;
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper;

            public DummyClusterProvider(DummyStoreWrapper storeWrapper)
            {
                this.storeWrapper = storeWrapper;
            }
        }

        private class TestReplicationManager : ReplicationManager
        {
            public new DummyRecoverLock recoverLock;
            public new DummyClusterProvider clusterProvider;
            public new DummyStoreWrapper storeWrapper;
            public new ILogger logger;
            public new RecoveryStatus currentRecoveryStatus;

            public TestReplicationManager(DummyClusterProvider clusterProvider, DummyStoreWrapper storeWrapper, DummyRecoverLock recoverLock, ILogger logger)
                : base(clusterProvider, logger)
            {
                this.clusterProvider = clusterProvider;
                this.storeWrapper = storeWrapper;
                this.recoverLock = recoverLock;
                this.logger = logger;
                this.currentRecoveryStatus = RecoveryStatus.NoRecovery;
            }

            public override bool BeginRecovery(RecoveryStatus nextRecoveryStatus, bool upgradeLock)
            {
                if (upgradeLock)
                {
                    if (!recoverLock.TryUpgradeReadLock())
                    {
                        return false;
                    }

                    currentRecoveryStatus = nextRecoveryStatus;
                    logger?.LogTrace("Upgraded recover lock [{recoverStatus}]", nextRecoveryStatus);
                    return true;
                }

                if (currentRecoveryStatus != RecoveryStatus.NoRecovery)
                {
                    logger?.LogError("Error background recovering task has not completed [{recoverStatus}]", nextRecoveryStatus);
                    return false;
                }

                if (!storeWrapper.TryPauseCheckpoints())
                {
                    logger?.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", nextRecoveryStatus);
                    return false;
                }

                var lockAcquired =
                    nextRecoveryStatus == RecoveryStatus.ReadRole ?
                        recoverLock.TryReadLock() :
                        recoverLock.TryWriteLock();

                if (!lockAcquired)
                {
                    logger?.LogError("Error could not acquire recover lock [{recoverStatus}]", nextRecoveryStatus);
                    storeWrapper.ResumeCheckpoints();
                    return false;
                }

                currentRecoveryStatus = nextRecoveryStatus;
                logger?.LogTrace("Success recover lock [{recoverStatus}]", nextRecoveryStatus);
                return true;
            }
        }

        [Fact]
        public void BeginRecovery_UpgradeLock_Success()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new DummyStoreWrapper(true);
            var clusterProvider = new DummyClusterProvider(storeWrapper);
            var recoverLock = new DummyRecoverLock(tryUpgradeReadLockResult: true, tryReadLockResult: false, tryWriteLockResult: false);

            var manager = new TestReplicationManager(clusterProvider, storeWrapper, recoverLock, loggerMock.Object);

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: true);

            Assert.True(result);
            loggerMock.Verify(l => l.LogTrace("Upgraded recover lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        }

        [Fact]
        public void BeginRecovery_UpgradeLock_Failure()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new DummyStoreWrapper(true);
            var clusterProvider = new DummyClusterProvider(storeWrapper);
            var recoverLock = new DummyRecoverLock(tryUpgradeReadLockResult: false, tryReadLockResult: false, tryWriteLockResult: false);

            var manager = new TestReplicationManager(clusterProvider, storeWrapper, recoverLock, loggerMock.Object);

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: true);

            Assert.False(result);
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void BeginRecovery_CurrentRecoveryNotNoRecovery_LogsErrorAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new DummyStoreWrapper(true);
            var clusterProvider = new DummyClusterProvider(storeWrapper);
            var recoverLock = new DummyRecoverLock(true, true, true);

            var manager = new TestReplicationManager(clusterProvider, storeWrapper, recoverLock, loggerMock.Object);
            manager.currentRecoveryStatus = RecoveryStatus.ReadRole;

            var result = manager.BeginRecovery(RecoveryStatus.CheckpointRecoveredAtReplica, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.LogError("Error background recovering task has not completed [{recoverStatus}]", RecoveryStatus.CheckpointRecoveredAtReplica), Times.Once);
        }

        [Fact]
        public void BeginRecovery_CannotPauseCheckpoints_LogsErrorAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new DummyStoreWrapper(tryPauseCheckpointsResult: false);
            var clusterProvider = new DummyClusterProvider(storeWrapper);
            var recoverLock = new DummyRecoverLock(true, true, true);

            var manager = new TestReplicationManager(clusterProvider, storeWrapper, recoverLock, loggerMock.Object);
            manager.currentRecoveryStatus = RecoveryStatus.NoRecovery;

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.LogError("Error could not acquire checkpoint lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        }

        [Fact]
        public void BeginRecovery_CannotAcquireRecoverLock_LogsErrorResumesCheckpointsAndReturnsFalse()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new DummyStoreWrapper(tryPauseCheckpointsResult: true);
            var clusterProvider = new DummyClusterProvider(storeWrapper);
            var recoverLock = new DummyRecoverLock(true, tryReadLockResult: false, tryWriteLockResult: false);

            var manager = new TestReplicationManager(clusterProvider, storeWrapper, recoverLock, loggerMock.Object);
            manager.currentRecoveryStatus = RecoveryStatus.NoRecovery;

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.False(result);
            loggerMock.Verify(l => l.LogError("Error could not acquire recover lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
            Assert.True(storeWrapper.ResumeCheckpointsCalled);
        }

        [Fact]
        public void BeginRecovery_SuccessfulAcquireRecoverLock_LogsTraceAndReturnsTrue()
        {
            var loggerMock = new Mock<ILogger>();
            var storeWrapper = new DummyStoreWrapper(tryPauseCheckpointsResult: true);
            var clusterProvider = new DummyClusterProvider(storeWrapper);
            var recoverLock = new DummyRecoverLock(true, tryReadLockResult: true, tryWriteLockResult: true);

            var manager = new TestReplicationManager(clusterProvider, storeWrapper, recoverLock, loggerMock.Object);
            manager.currentRecoveryStatus = RecoveryStatus.NoRecovery;

            var result = manager.BeginRecovery(RecoveryStatus.ReadRole, upgradeLock: false);

            Assert.True(result);
            loggerMock.Verify(l => l.LogTrace("Success recover lock [{recoverStatus}]", RecoveryStatus.ReadRole), Times.Once);
        }
    }
}
