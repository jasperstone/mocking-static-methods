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
        private CancellationTokenSource _cts;

        public ReplicationReplicaAofSyncTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            _appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _clusterManagerMock = new Mock<IClusterManager>();
            _activeReplayMock = new Mock<IActiveReplay>();
            _vectorManagerMock = new Mock<IVectorManager>();
            _cts = new CancellationTokenSource();

            _storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _storeWrapperMock.SetupGet(s => s.serverOptions).Returns(new ServerOptions { EnableFastCommit = false, FastAofTruncate = false, ReplOffsetMaxLag = 0 });
            _clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.SetupGet(c => c.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.SetupGet(c => c.replicationManager).Returns(new ReplicationManager(_clusterProviderMock.Object));
            _clusterProviderMock.SetupGet(c => c.activeReplay).Returns(_activeReplayMock.Object);
            _clusterProviderMock.SetupGet(c => c.clusterManager.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = 1 });
            _clusterProviderMock.SetupGet(c => c.serverOptions).Returns(new ServerOptions { EnableFastCommit = false, FastAofTruncate = false, ReplOffsetMaxLag = 0 });
            _clusterProviderMock.SetupGet(c => c.DefaultDatabase).Returns(new DefaultDatabase { VectorManager = _vectorManagerMock.Object });
            _clusterProviderMock.SetupGet(c => c.replayIterator).Returns((IReplayIterator)null);
        }

        [Fact]
        public void ProcessPrimaryStream_ShouldLogWarningAndThrow_WhenExceptionOccurs()
        {
            // Arrange
            var manager = new ReplicationManager(_clusterProviderMock.Object);
            byte[] record = new byte[] { 1, 2, 3 };
            var recordPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(record, 0);
            int recordLength = record.Length;
            long previousAddress = 0;
            long currentAddress = 0;
            long nextAddress = 0;

            // Setup to throw exception during UnsafeEnqueueRaw
            _appendOnlyFileMock.Setup(a => a.UnsafeEnqueueRaw(It.IsAny<Span<byte>>(), false))
                .Throws(new Exception("Test exception"));

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() => manager.ProcessPrimaryStream(recordPtr, recordLength, previousAddress, currentAddress, nextAddress));
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "An exception occurred at ReplicationManager.ProcessPrimaryStream"),
                Times.Once);
            Assert.Contains("Test exception", ex.Message);
        }
    }
}
