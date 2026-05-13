using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        private Mock<ILogger<ReplicationManager>> _loggerMock;
        private Mock<IStoreWrapper> _storeWrapperMock;
        private Mock<IAppendOnlyFile> _appendOnlyFileMock;
        private Mock<IClusterProvider> _clusterProviderMock;
        private Mock<IClusterManager> _clusterManagerMock;
        private Mock<IActiveReplay> _activeReplayMock;
        private Mock<IVectorManager> _vectorManagerMock;
        private Mock<IDefaultDatabase> _defaultDatabaseMock;
        private ReplicationManager _replicationManager;

        public ReplicationReplicaAofSyncTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            _appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _clusterManagerMock = new Mock<IClusterManager>();
            _activeReplayMock = new Mock<IActiveReplay>();
            _vectorManagerMock = new Mock<IVectorManager>();
            _defaultDatabaseMock = new Mock<IDefaultDatabase>();

            _storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _storeWrapperMock.SetupGet(s => s.serverOptions).Returns(new ServerOptions { EnableFastCommit = false, FastAofTruncate = false, ReplcationOffsetMaxLag = 0 });
            _storeWrapperMock.SetupGet(s => s.DefaultDatabase).Returns(_defaultDatabaseMock.Object);

            _clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.SetupGet(c => c.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.SetupGet(c => c.replicationManager).Returns(new Mock<IReplicationManager>().Object);
            _clusterProviderMock.SetupGet(c => c.serverOptions).Returns(new ServerOptions { EnableFastCommit = false, FastAofTruncate = false, ReplcationOffsetMaxLag = 0 });
            _clusterProviderMock.SetupGet(c => c.activeReplay).Returns(_activeReplayMock.Object);
            _clusterProviderMock.SetupGet(c => c.DefaultDatabase).Returns(_defaultDatabaseMock.Object);

            _clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" });

            _activeReplayMock.Setup(a => a.TryReadLock()).Returns(true);

            _replicationManager = new ReplicationManager
            {
                clusterProvider = _clusterProviderMock.Object,
                logger = _loggerMock.Object,
                replayIterator = null,
                pageSizeBits = 12,
                ReplicationOffset = 0,
                storeWrapper = _storeWrapperMock.Object,
                activeReplay = _activeReplayMock.Object,
                clusterProvider = _clusterProviderMock.Object
            };
        }

        [Fact]
        public void ProcessPrimaryStream_ShouldLogWarningAndThrow_WhenExceptionInjected()
        {
            // Arrange
            byte[] record = new byte[] { 1, 2, 3, 4 };
            int recordLength = record.Length;
            long previousAddress = 0;
            long currentAddress = 0;
            long nextAddress = 0;

            _storeWrapperMock.Setup(s => s.appendOnlyFile.TailAddress).Returns(0);
            _storeWrapperMock.Setup(s => s.appendOnlyFile.SafeInitialize(It.IsAny<long>(), It.IsAny<long>())).Verifiable();
            _appendOnlyFileMock.Setup(a => a.UnsafeEnqueueRaw(It.IsAny<Span<byte>>(), false)).Returns(true);
            _appendOnlyFileMock.Setup(a => a.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, null)).Returns((IAppendOnlyFile.ScanIterator)null);

            // Inject exception on trigger
            ExceptionInjectionHelper.TriggerException(ExceptionInjectionType.Divergent_AOF_Stream);

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() =>
                _replicationManager.ProcessPrimaryStream(record.AsSpan().ToPointer(), recordLength, previousAddress, currentAddress, nextAddress));

            // Verify that LogWarning was called with the exception
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "An exception occurred at ReplicationManager.ProcessPrimaryStream"),
                Times.Once);
        }
    }
}
