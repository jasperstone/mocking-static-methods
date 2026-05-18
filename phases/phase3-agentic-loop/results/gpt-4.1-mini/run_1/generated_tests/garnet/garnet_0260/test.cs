using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    internal class ReplicationManagerTests
    {
        // Minimal mocks for dependencies
        private class DummyStoreWrapper
        {
            public bool TryPauseCheckpointsReturn = true;
            public bool ResumeCheckpointsCalledFlag = false;

            public bool TryPauseCheckpoints() => TryPauseCheckpointsReturn;
            public void ResumeCheckpoints() => ResumeCheckpointsCalledFlag = true;
        }

        private class DummyClusterManager
        {
            public DummyConfig CurrentConfig { get; } = new DummyConfig();
        }

        private class DummyConfig
        {
            public NodeRole LocalNodeRole { get; set; } = NodeRole.REPLICA;
        }

        private class DummyServerOptions
        {
            public bool Recover { get; set; } = true;
            public bool EnableAOF { get; set; } = false;
            public bool DisableObjects { get; set; } = true;
        }

        private class DummyClusterProvider
        {
            public DummyStoreWrapper storeWrapper;
            public DummyClusterManager clusterManager;
            public DummyServerOptions serverOptions;

            public DummyClusterProvider()
            {
                storeWrapper = new DummyStoreWrapper();
                clusterManager = new DummyClusterManager();
                serverOptions = new DummyServerOptions();
            }
        }

        private class DummySingleWriterMultiReaderLock
        {
            public bool TryUpgradeReadLockReturn = true;
            public bool TryReadLockReturn = true;
            public bool TryWriteLockReturn = true;

            public bool TryUpgradeReadLock() => TryUpgradeReadLockReturn;
            public bool TryReadLock() => TryReadLockReturn;
            public bool TryWriteLock() => TryWriteLockReturn;
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        private object CreateReplicationManager(DummyClusterProvider clusterProvider, ILogger logger)
        {
            var rmType = typeof(ReplicationManager);
            var ctor = rmType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { clusterProvider.GetType(), typeof(ILogger) }, null);
            if (ctor == null)
            {
                ctor = rmType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object), typeof(ILogger) }, null);
            }
            if (ctor == null)
            {
                ctor = rmType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            }
            if (ctor == null)
                throw new Exception("Could not find ReplicationManager constructor");

            object instance = null;
            try
            {
                instance = Activator.CreateInstance(rmType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { null, logger }, null);
            }
            catch
            {
                instance = Activator.CreateInstance(rmType, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            }
            return instance;
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryNotNoRecovery()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var rm = CreateReplicationManager(clusterProvider, loggerMock.Object);

            SetPrivateField(rm, "currentRecoveryStatus", RecoveryStatus.InitializeRecover);
            SetPrivateField(rm, "clusterProvider", clusterProvider);
            SetPrivateField(rm, "storeWrapper", clusterProvider.storeWrapper);
            SetPrivateField(rm, "logger", loggerMock.Object);

            var method = rm.GetType().GetMethod("BeginRecovery", BindingFlags.Public | BindingFlags.Instance);
            var result = (bool)method.Invoke(rm, new object[] { RecoveryStatus.ReadRole, false });

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
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
            var clusterProvider = new DummyClusterProvider();
            clusterProvider.storeWrapper.TryPauseCheckpointsReturn = false;
            var rm = CreateReplicationManager(clusterProvider, loggerMock.Object);

            SetPrivateField(rm, "currentRecoveryStatus", RecoveryStatus.NoRecovery);
            SetPrivateField(rm, "clusterProvider", clusterProvider);
            SetPrivateField(rm, "storeWrapper", clusterProvider.storeWrapper);
            SetPrivateField(rm, "logger", loggerMock.Object);

            var method = rm.GetType().GetMethod("BeginRecovery", BindingFlags.Public | BindingFlags.Instance);
            var result = (bool)method.Invoke(rm, new object[] { RecoveryStatus.ReadRole, false });

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire checkpoint lock")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenTryAcquireRecoverLockFails_AndResumesCheckpoints()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            clusterProvider.storeWrapper.TryPauseCheckpointsReturn = true;
            var rm = CreateReplicationManager(clusterProvider, loggerMock.Object);

            SetPrivateField(rm, "currentRecoveryStatus", RecoveryStatus.NoRecovery);
            SetPrivateField(rm, "clusterProvider", clusterProvider);
            SetPrivateField(rm, "storeWrapper", clusterProvider.storeWrapper);
            SetPrivateField(rm, "logger", loggerMock.Object);

            var dummyLock = new DummySingleWriterMultiReaderLock
            {
                TryReadLockReturn = false,
                TryWriteLockReturn = false
            };
            SetPrivateField(rm, "recoverLock", dummyLock);

            var method = rm.GetType().GetMethod("BeginRecovery", BindingFlags.Public | BindingFlags.Instance);
            var result = (bool)method.Invoke(rm, new object[] { RecoveryStatus.ReadRole, false });

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error could not acquire recover lock")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(clusterProvider.storeWrapper.ResumeCheckpointsCalledFlag);
        }
    }
}
