using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        private readonly Mock<ILogger<ReplicationManager>> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<AppendOnlyFile> _appendOnlyFileMock;
        private readonly Mock<DefaultDatabase> _defaultDatabaseMock;
        private readonly Mock<VectorManager> _vectorManagerMock;
        private readonly Mock<ActiveReplay> _activeReplayMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;

        public ReplicationReplicaAofSyncTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _appendOnlyFileMock = new Mock<AppendOnlyFile>();
            _defaultDatabaseMock = new Mock<DefaultDatabase>();
            _vectorManagerMock = new Mock<VectorManager>();
            _activeReplayMock = new Mock<ActiveReplay>();
            _replicationManagerMock = new Mock<ReplicationManager>();

            // Setup default behaviors
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _storeWrapperMock.Setup(sw => sw.DefaultDatabase).Returns(_defaultDatabaseMock.Object);
            _defaultDatabaseMock.Setup(db => db.VectorManager).Returns(_vectorManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(new ClusterManager { CurrentConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = 1 } });
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { FastAofTruncate = false, EnableFastCommit = false, ReplicationOffsetMaxLag = 10 });
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.activeReplay).Returns(_activeReplayMock.Object);
            _clusterProviderMock.Setup(cp => cp.ReplicationOffset).Returns(0);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(new ClusterManager { CurrentConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = 1 } });
            _clusterProviderMock.Setup(cp => cp.ReplicationOffset).Returns(0);
            _clusterProviderMock.Setup(cp => cp.storeWrapper.appendOnlyFile.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, null))
                .Returns(new List<long> { 1, 2, 3 });
        }

        [Fact]
        public void ProcessPrimaryStream_Should_LogWarningAndThrow_When_ExceptionInjected()
        {
            // Arrange
            var record = new byte[] { 1, 2, 3, 4 };
            var recordPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(record, 0);
            var pm = new ReplicationManager
            {
                clusterProvider = _clusterProviderMock.Object,
                logger = _loggerMock.Object,
                storeWrapper = _storeWrapperMock.Object,
                activeReplay = _activeReplayMock.Object,
                replayIterator = null,
                pageSizeBits = 12,
                ReplicationOffset = 0
            };

            // Inject exception
            ExceptionInjectionHelper.TriggerException(ExceptionInjectionType.Divergent_AOF_Stream);

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() =>
                pm.ProcessPrimaryStream(recordPtr, record.Length, 0, 0, 0));
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "An exception occurred at ReplicationManager.ProcessPrimaryStream"),
                Times.Once);
        }
    }

    // Dummy classes to support the test
    public class ClusterProvider
    {
        public StoreWrapper storeWrapper { get; set; }
        public ClusterManager clusterManager { get; set; }
        public ServerOptions serverOptions { get; set; }
        public ActiveReplay activeReplay { get; set; }
        public int ReplicationOffset { get; set; }
        public ReplicationManager replicationManager { get; set; }
    }

    public class StoreWrapper
    {
        public AppendOnlyFile appendOnlyFile { get; set; }
        public DefaultDatabase DefaultDatabase { get; set; }
    }

    public class AppendOnlyFile
    {
        public long TailAddress { get; set; }
        public List<long> ScanSingle(long previousAddress, long maxAddress, bool scanUncommitted, bool recover, object logger) => new List<long>();
        public void SafeInitialize(long currentAddress, long currentAddress2) { }
        public object UnsafeEnqueueRaw(Span<byte> span, bool noCommit) => null;
    }

    public class DefaultDatabase
    {
        public VectorManager VectorManager { get; set; }
    }

    public class VectorManager
    {
        public void WaitForVectorOperationsToComplete() { }
    }

    public class ActiveReplay
    {
        public bool TryReadLock() => true;
    }

    public class ClusterManager
    {
        public ClusterConfig CurrentConfig { get; set; }
    }

    public class ClusterConfig
    {
        public NodeRole LocalNodeRole { get; set; }
        public int LocalNodeId { get; set; }
    }

    public enum NodeRole
    {
        REPLICA,
        PRIMARY
    }

    public class ServerOptions
    {
        public bool FastAofTruncate { get; set; }
        public bool EnableFastCommit { get; set; }
        public int ReplicationOffsetMaxLag { get; set; }
    }

    public static class ExceptionInjectionHelper
    {
        public static void TriggerException(ExceptionInjectionType type) { }
    }

    public enum ExceptionInjectionType
    {
        Divergent_AOF_Stream
    }
}
