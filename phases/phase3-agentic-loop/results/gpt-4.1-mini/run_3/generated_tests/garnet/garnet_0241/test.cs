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
        // Helper to create a ReplicationManager with mocked dependencies
        private ReplicationManager CreateReplicationManager(
            bool cannotStreamAof = false,
            NodeRole localNodeRole = NodeRole.REPLICA,
            long replicationOffsetMaxLag = 1,
            long tailAddress = 0,
            long replicationOffset = 0,
            bool fastAofTruncate = false)
        {
            var loggerMock = new Mock<ILogger>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var clusterConfig = new ClusterConfig { LocalNodeRole = localNodeRole, LocalNodeId = "node1" };
            clusterManagerMock.Setup(c => c.CurrentConfig).Returns(clusterConfig);

            var replicationManagerMock = new Mock<ReplicationManager>(MockBehavior.Loose);
            replicationManagerMock.CallBase = true;

            var storeWrapperMock = new Mock<IStoreWrapper>();
            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            appendOnlyFileMock.SetupGet(a => a.TailAddress).Returns(tailAddress);
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(new ServerOptions
            {
                ReplicationOffsetMaxLag = replicationOffsetMaxLag,
                FastAofTruncate = fastAofTruncate,
                EnableFastCommit = false
            });
            storeWrapperMock.SetupGet(s => s.DefaultDatabase).Returns(null);

            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(storeWrapperMock.Object.serverOptions);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);

            replicationManagerMock.Object.logger = loggerMock.Object;
            replicationManagerMock.Object.clusterProvider = clusterProviderMock.Object;
            replicationManagerMock.Object.activeReplay = new ActiveReplayMock();
            replicationManagerMock.Object.ReplicationOffset = replicationOffset;

            // Setup CannotStreamAOF property
            replicationManagerMock.SetupGet(r => r.CannotStreamAOF).Returns(cannotStreamAof);

            return replicationManagerMock.Object;
        }

        // Mock for activeReplay.TryReadLock to always succeed
        private class ActiveReplayMock
        {
            public bool TryReadLock() => true;
        }

        // Dummy interfaces and classes to satisfy dependencies
        private interface IClusterManager
        {
            ClusterConfig CurrentConfig { get; }
        }

        private class ClusterConfig
        {
            public NodeRole LocalNodeRole { get; set; }
            public string LocalNodeId { get; set; }
        }

        private enum NodeRole
        {
            REPLICA,
            PRIMARY
        }

        private interface IStoreWrapper
        {
            IAppendOnlyFile appendOnlyFile { get; }
            ServerOptions serverOptions { get; }
            IDatabase DefaultDatabase { get; }
        }

        private interface IAppendOnlyFile
        {
            long TailAddress { get; }
            void SafeInitialize(long start, long end);
            bool UnsafeEnqueueRaw(Span<byte> data, bool noCommit);
            IReplayIterator ScanSingle(long previousAddress, long currentAddress);
        }

        private interface IReplayIterator { }

        private interface IDatabase
        {
            IVectorManager VectorManager { get; }
        }

        private interface IVectorManager
        {
            void WaitForVectorOperationsToComplete();
        }

        private class ServerOptions
        {
            public long ReplicationOffsetMaxLag { get; set; }
            public bool FastAofTruncate { get; set; }
            public bool EnableFastCommit { get; set; }
        }

        private interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
            ReplicationManager replicationManager { get; }
            ServerOptions serverOptions { get; }
            IStoreWrapper storeWrapper { get; }
        }

        [Fact]
        public unsafe void ProcessPrimaryStream_LogsErrorAndThrows_WhenCannotStreamAOF()
        {
            // Arrange
            var replicationManager = CreateReplicationManager(cannotStreamAof: true);
            var loggerMock = Mock.Get(replicationManager.logger);

            byte[] dummyData = new byte[1];
            fixed (byte* ptr = dummyData)
            {
                // Act & Assert
                var ex = Assert.Throws<GarnetException>(() =>
                    replicationManager.ProcessPrimaryStream(ptr, dummyData.Length, 0, 0, 0));

                Assert.Contains("Replica is recovering cannot sync AOF", ex.Message);

                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
