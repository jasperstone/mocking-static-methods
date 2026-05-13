using System;
using System.Threading;
using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        private Mock<ILogger<ReplicationManager>> _loggerMock;
        private Mock<IClusterProvider> _clusterProviderMock;
        private Mock<IStoreWrapper> _storeWrapperMock;
        private Mock<IAppendOnlyFile> _appendOnlyFileMock;
        private Mock<IActiveReplay> _activeReplayMock;
        private Mock<IClusterManager> _clusterManagerMock;
        private Mock<IConfig> _configMock;
        private Mock<IVectorManager> _vectorManagerMock;

        public ReplicationReplicaAofSyncTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            _appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            _activeReplayMock = new Mock<IActiveReplay>();
            _clusterManagerMock = new Mock<IClusterManager>();
            _configMock = new Mock<IConfig>();
            _vectorManagerMock = new Mock<IVectorManager>();

            // Setup default behaviors
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, EnableFastCommit = false, FastAofTruncate = false });
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _storeWrapperMock.Setup(sw => sw.DefaultDatabase).Returns(new Database { VectorManager = _vectorManagerMock.Object });
            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(new Config { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" });
            _activeReplayMock.Setup(ar => ar.TryReadLock()).Returns(true);
            _storeWrapperMock.Setup(sw => sw.ReplayIterator).Returns((IReplayIterator)null);
        }

        [Fact]
        public void ProcessPrimaryStream_ShouldLogErrorAndThrow_WhenReplicaIsRecovering()
        {
            // Arrange
            var manager = new ReplicationManager
            {
                clusterProvider = _clusterProviderMock.Object,
                logger = _loggerMock.Object,
                storeWrapper = _storeWrapperMock.Object,
                replayIterator = null,
                ReplicationOffset = 0,
                pageSizeBits = 12
            };

            // Simulate replica is recovering
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(new Mock<IReplicationManager>().Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager.CannotStreamAOF).Returns(true);

            byte[] recordBytes = new byte[] { 1, 2, 3, 4 };
            unsafe
            {
                fixed (byte* recordPtr = recordBytes)
                {
                    // Act & Assert
                    var ex = Assert.Throws<GarnetException>(() =>
                        manager.ProcessPrimaryStream(recordPtr, recordBytes.Length, 0, 0, 0));
                    Assert.Contains("Replica is recovering cannot sync AOF", ex.Message);
                }
            }

            // Verify that LogError was called
            _loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
