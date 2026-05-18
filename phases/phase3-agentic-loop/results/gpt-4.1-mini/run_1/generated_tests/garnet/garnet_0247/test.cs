using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.common;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        // We cannot inherit ReplicationManager because it is internal sealed.
        // Instead, we will use reflection or internal access to create an instance.
        // But since the class is internal sealed, we cannot instantiate it directly here.
        // So we will create a minimal test class that mocks ILogger and calls the method via reflection.
        // We will simulate the failReplay condition by mocking dependencies and forcing TryReadLock to return false.

        // Interfaces to mock dependencies (simplified)
        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
            IServerOptions serverOptions { get; }
            IReplicationManager replicationManager { get; }
            IStoreWrapper storeWrapper { get; }
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

        public interface IServerOptions
        {
            int ReplicationOffsetMaxLag { get; }
            bool EnableFastCommit { get; }
            bool FastAofTruncate { get; }
        }

        public interface IReplicationManager
        {
            bool CannotStreamAOF { get; }
        }

        public interface IStoreWrapper
        {
            IAppendOnlyFile appendOnlyFile { get; }
            IDefaultDatabase DefaultDatabase { get; }
            IServerOptions serverOptions { get; }
        }

        public interface IAppendOnlyFile
        {
            long TailAddress { get; }
            void SafeInitialize(long start, long end);
            void UnsafeEnqueueRaw(Span<byte> data, bool noCommit);
            IReplayIterator ScanSingle(long start, long end, bool scanUncommitted, bool recover, ILogger logger);
        }

        public interface IDefaultDatabase
        {
            IVectorManager VectorManager { get; }
        }

        public interface IVectorManager
        {
            void WaitForVectorOperationsToComplete();
        }

        public interface IReplayIterator { }

        public interface IActiveReplay
        {
            bool TryReadLock();
            void ReadUnlock();
        }

        [Fact]
        public unsafe void ProcessPrimaryStream_LogsWarningOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var serverOptionsMock = new Mock<IServerOptions>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var currentConfigMock = new Mock<ICurrentConfig>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var activeReplayMock = new Mock<IActiveReplay>();
            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            var defaultDatabaseMock = new Mock<IDefaultDatabase>();
            var vectorManagerMock = new Mock<IVectorManager>();

            // Setup server options to enable syncReplay (ReplicationOffsetMaxLag == 0)
            serverOptionsMock.SetupGet(s => s.ReplicationOffsetMaxLag).Returns(0);
            serverOptionsMock.SetupGet(s => s.EnableFastCommit).Returns(false);
            serverOptionsMock.SetupGet(s => s.FastAofTruncate).Returns(false);

            // Setup clusterProvider
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);

            // Setup currentConfig to be replica role
            currentConfigMock.SetupGet(c => c.LocalNodeRole).Returns(NodeRole.REPLICA);
            currentConfigMock.SetupGet(c => c.LocalNodeId).Returns("node-1");
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(currentConfigMock.Object);

            // Setup replicationManager to allow streaming
            replicationManagerMock.SetupGet(r => r.CannotStreamAOF).Returns(false);

            // Setup storeWrapper
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.SetupGet(s => s.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            defaultDatabaseMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);

            // Setup activeReplay to fail TryReadLock to force exception and logging
            activeReplayMock.Setup(a => a.TryReadLock()).Returns(false);

            // Create instance of ReplicationManager via reflection
            var replicationManagerType = typeof(ReplicationManager);
            var ctor = replicationManagerType.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);
            Assert.NotNull(ctor);
            var replicationManager = ctor.Invoke(null);

            // Set private fields via reflection
            var clusterProviderField = replicationManagerType.GetField("clusterProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var storeWrapperField = replicationManagerType.GetField("storeWrapper", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var activeReplayField = replicationManagerType.GetField("activeReplay", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var loggerField = replicationManagerType.GetField("logger", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var pageSizeBitsField = replicationManagerType.GetField("pageSizeBits", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var ReplicationOffsetField = replicationManagerType.GetField("ReplicationOffset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            clusterProviderField.SetValue(replicationManager, clusterProviderMock.Object);
            storeWrapperField.SetValue(replicationManager, storeWrapperMock.Object);
            activeReplayField.SetValue(replicationManager, activeReplayMock.Object);
            loggerField.SetValue(replicationManager, loggerMock.Object);
            pageSizeBitsField.SetValue(replicationManager, 12);
            ReplicationOffsetField.SetValue(replicationManager, 0L);

            // Prepare dummy data for unsafe pointer
            byte[] dummyData = new byte[10];
            fixed (byte* p = dummyData)
            {
                // Act & Assert
                var ex = Assert.Throws<GarnetException>(() =>
                {
                    var method = replicationManagerType.GetMethod("ProcessPrimaryStream", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    method.Invoke(replicationManager, new object[] { (IntPtr)p, dummyData.Length, 0L, 0L, 0L });
                });

                // Verify logger.LogWarning was called with exception and expected message
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
