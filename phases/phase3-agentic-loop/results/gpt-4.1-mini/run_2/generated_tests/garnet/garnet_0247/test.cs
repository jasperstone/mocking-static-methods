using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.common;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        // Mock interfaces to satisfy dependencies
        public interface IClusterProvider
        {
            IStoreWrapper storeWrapper { get; }
            IServerOptions serverOptions { get; }
            IClusterManager clusterManager { get; }
            IReplicationManager replicationManager { get; }
        }

        public interface IStoreWrapper
        {
            IAppendOnlyFile appendOnlyFile { get; }
            IServerOptions serverOptions { get; }
            IDatabase DefaultDatabase { get; }
        }

        public interface IAppendOnlyFile
        {
            long TailAddress { get; }
            void SafeInitialize(long start, long end);
            void UnsafeEnqueueRaw(ReadOnlySpan<byte> data, bool noCommit);
            object ScanSingle(long start, long end, bool scanUncommitted, bool recover, ILogger logger);
        }

        public interface IServerOptions
        {
            int ReplicationOffsetMaxLag { get; }
            bool EnableFastCommit { get; }
            bool FastAofTruncate { get; }
        }

        public interface IClusterManager
        {
            ICurrentConfig CurrentConfig { get; }
        }

        public interface ICurrentConfig
        {
            NodeRole LocalNodeRole { get; }
            string LocalNodeId { get; }
        }

        public interface IReplicationManager
        {
            bool CannotStreamAOF { get; }
        }

        public interface IDatabase
        {
            IVectorManager VectorManager { get; }
        }

        public interface IVectorManager
        {
            void WaitForVectorOperationsToComplete();
        }

        public enum NodeRole
        {
            REPLICA,
            PRIMARY
        }

        // We create a minimal wrapper class to call ProcessPrimaryStream via reflection
        // and inject mocks to cause an exception and verify LogWarning call.
        [Fact]
        public unsafe void ProcessPrimaryStream_LogsWarningOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            appendOnlyFileMock.SetupGet(a => a.TailAddress).Returns(0);
            appendOnlyFileMock.Setup(a => a.UnsafeEnqueueRaw(It.IsAny<ReadOnlySpan<byte>>(), It.IsAny<bool>()));

            var vectorManagerMock = new Mock<IVectorManager>();
            vectorManagerMock.Setup(v => v.WaitForVectorOperationsToComplete());

            var defaultDatabaseMock = new Mock<IDatabase>();
            defaultDatabaseMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);

            var serverOptionsMock = new Mock<IServerOptions>();
            serverOptionsMock.SetupGet(s => s.ReplicationOffsetMaxLag).Returns(0);
            serverOptionsMock.SetupGet(s => s.EnableFastCommit).Returns(false);
            serverOptionsMock.SetupGet(s => s.FastAofTruncate).Returns(false);

            var storeWrapperMock = new Mock<IStoreWrapper>();
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.SetupGet(s => s.DefaultDatabase).Returns(defaultDatabaseMock.Object);

            var currentConfigMock = new Mock<ICurrentConfig>();
            currentConfigMock.SetupGet(c => c.LocalNodeRole).Returns(NodeRole.REPLICA);
            currentConfigMock.SetupGet(c => c.LocalNodeId).Returns("node1");

            var clusterManagerMock = new Mock<IClusterManager>();
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(currentConfigMock.Object);

            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.SetupGet(r => r.CannotStreamAOF).Returns(false);

            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);

            // Create instance of ReplicationManager via reflection
            var replicationManagerType = typeof(ReplicationManager);
            var replicationManagerCtor = replicationManagerType.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);
            Assert.NotNull(replicationManagerCtor);
            var replicationManager = replicationManagerCtor.Invoke(null);

            // Set private fields via reflection
            var clusterProviderField = replicationManagerType.GetField("clusterProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var storeWrapperField = replicationManagerType.GetField("storeWrapper", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var loggerField = replicationManagerType.GetField("logger", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var activeReplayField = replicationManagerType.GetField("activeReplay", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var replicaReplayTaskCtsField = replicationManagerType.GetField("replicaReplayTaskCts", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var pageSizeBitsField = replicationManagerType.GetField("pageSizeBits", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var ReplicationOffsetField = replicationManagerType.GetField("ReplicationOffset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var replayIteratorField = replicationManagerType.GetField("replayIterator", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            clusterProviderField.SetValue(replicationManager, clusterProviderMock.Object);
            storeWrapperField.SetValue(replicationManager, storeWrapperMock.Object);
            loggerField.SetValue(replicationManager, loggerMock.Object);
            replicaReplayTaskCtsField.SetValue(replicationManager, new CancellationTokenSource());
            pageSizeBitsField.SetValue(replicationManager, 12);
            ReplicationOffsetField.SetValue(replicationManager, 0);
            replayIteratorField.SetValue(replicationManager, null);

            // Setup activeReplay mock with TryReadLock returning false to cause exception and trigger catch block
            var activeReplayMockType = replicationManagerType.Assembly.GetType("Garnet.cluster.ActiveReplay");
            var activeReplayMock = Activator.CreateInstance(activeReplayMockType);
            var tryReadLockMethod = activeReplayMockType.GetMethod("TryReadLock");

            // Create a delegate to override TryReadLock to return false
            var tryReadLockDelegate = new Func<bool>(() => false);
            // We cannot override method, so we use a proxy object with a field to indicate failure
            // Instead, set activeReplay to a custom object with TryReadLock method returning false via dynamic proxy is complicated,
            // so we set activeReplay to null to cause NullReferenceException and trigger catch block for logging.

            activeReplayField.SetValue(replicationManager, null);

            // Prepare dummy data
            byte[] data = new byte[10];
            fixed (byte* pData = data)
            {
                // Act & Assert
                var processMethod = replicationManagerType.GetMethod("ProcessPrimaryStream", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                var ex = Assert.ThrowsAny<Exception>(() =>
                {
                    processMethod.Invoke(replicationManager, new object[] { (IntPtr)pData, data.Length, 0L, 0L, 0L });
                });

                // Verify that LogWarning was called with the exception message
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An exception occurred at ReplicationManager.ProcessPrimaryStream")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
