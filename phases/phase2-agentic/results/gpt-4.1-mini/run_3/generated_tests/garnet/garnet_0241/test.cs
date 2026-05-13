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
        // Helper class to expose unsafe method for testing
        private unsafe class TestReplicationManager : ReplicationManager
        {
            public TestReplicationManager(
                IClusterProvider clusterProvider,
                ILogger logger,
                IActiveReplay activeReplay,
                IStoreWrapper storeWrapper,
                int pageSizeBits = 12)
            {
                this.clusterProvider = clusterProvider;
                this.logger = logger;
                this.activeReplay = activeReplay;
                this.storeWrapper = storeWrapper;
                this.pageSizeBits = pageSizeBits;
                this.ReplicationOffset = 0;
            }

            public new unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
            {
                base.ProcessPrimaryStream(record, recordLength, previousAddress, currentAddress, nextAddress);
            }

            public ILogger logger;
            public IClusterProvider clusterProvider;
            public IActiveReplay activeReplay;
            public IStoreWrapper storeWrapper;
            public int pageSizeBits;
            public long ReplicationOffset;
        }

        // Interfaces to mock dependencies
        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
            IServerOptions serverOptions { get; }
            IReplicationManager replicationManager { get; }
            IStoreWrapper storeWrapper { get; }
        }

        public interface IClusterManager
        {
            IClusterConfig CurrentConfig { get; }
        }

        public interface IClusterConfig
        {
            NodeRole LocalNodeRole { get; }
            string LocalNodeId { get; }
        }

        public interface IServerOptions
        {
            int ReplicationOffsetMaxLag { get; }
            bool FastAofTruncate { get; }
            bool EnableFastCommit { get; }
        }

        public interface IReplicationManager
        {
            bool CannotStreamAOF { get; }
        }

        public interface IStoreWrapper
        {
            IAppendOnlyFile appendOnlyFile { get; }
            IDatabase DefaultDatabase { get; }
            IServerOptions serverOptions { get; }
        }

        public interface IAppendOnlyFile
        {
            long TailAddress { get; }
            void SafeInitialize(long start, long end);
            void UnsafeEnqueueRaw(Span<byte> data, bool noCommit);
            IReplayIterator ScanSingle(long previousAddress, long currentAddress);
        }

        public interface IDatabase
        {
            IVectorManager VectorManager { get; }
        }

        public interface IVectorManager
        {
            void WaitForVectorOperationsToComplete();
        }

        public interface IActiveReplay
        {
            bool TryReadLock();
        }

        public interface IReplayIterator { }

        [Fact]
        public unsafe void ProcessPrimaryStream_LogsErrorAndThrows_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var activeReplayMock = new Mock<IActiveReplay>();
            activeReplayMock.Setup(ar => ar.TryReadLock()).Returns(true);

            var clusterConfigMock = new Mock<IClusterConfig>();
            clusterConfigMock.SetupGet(c => c.LocalNodeRole).Returns(NodeRole.REPLICA);
            clusterConfigMock.SetupGet(c => c.LocalNodeId).Returns("node1");

            var clusterManagerMock = new Mock<IClusterManager>();
            clusterManagerMock.SetupGet(cm => cm.CurrentConfig).Returns(clusterConfigMock.Object);

            var serverOptionsMock = new Mock<IServerOptions>();
            serverOptionsMock.SetupGet(so => so.ReplicationOffsetMaxLag).Returns(1);
            serverOptionsMock.SetupGet(so => so.FastAofTruncate).Returns(false);
            serverOptionsMock.SetupGet(so => so.EnableFastCommit).Returns(false);

            var replicationManagerMock = new Mock<IReplicationManager>();
            replicationManagerMock.SetupGet(rm => rm.CannotStreamAOF).Returns(true);

            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            appendOnlyFileMock.SetupGet(aof => aof.TailAddress).Returns(0);

            var defaultDatabaseMock = new Mock<IDatabase>();
            var vectorManagerMock = new Mock<IVectorManager>();
            defaultDatabaseMock.SetupGet(db => db.VectorManager).Returns(vectorManagerMock.Object);

            var storeWrapperMock = new Mock<IStoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.SetupGet(sw => sw.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(serverOptionsMock.Object);

            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(cp => cp.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);

            var replicationManager = new TestReplicationManager(
                clusterProviderMock.Object,
                loggerMock.Object,
                activeReplayMock.Object,
                storeWrapperMock.Object);

            // Prepare dummy record pointer
            byte[] dummyData = new byte[1];
            fixed (byte* p = dummyData)
            {
                // Act & Assert
                var ex = Assert.Throws<GarnetException>(() =>
                    replicationManager.ProcessPrimaryStream(p, 1, 0, 0, 0));

                // Verify logger.LogError was called with expected message
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);

                Assert.Contains("Replica is recovering cannot sync AOF", ex.Message);
            }
        }
    }
}
