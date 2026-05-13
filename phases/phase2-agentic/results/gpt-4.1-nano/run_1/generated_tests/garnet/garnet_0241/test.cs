using System;
using System.Collections.Generic;
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
        private Mock<IReplicationManager> _replicationManagerMock;
        private Mock<IClusterManager> _clusterManagerMock;
        private Mock<IReplicationManager> _replicationManager;
        private Mock<IActiveReplay> _activeReplayMock;
        private Mock<IStoreWrapper> _storeWrapper;
        private Mock<IClusterProvider> _clusterProvider;
        private ReplicationManager _replicationManagerInstance;
        private Mock<ILogger> _logger;

        public ReplicationReplicaAofSyncTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            _appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            _activeReplayMock = new Mock<IActiveReplay>();
            _clusterManagerMock = new Mock<IClusterManager>();
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, FastAofTruncate = false, EnableFastCommit = false });
            _clusterProviderMock.Setup(cp => cp.DefaultDatabase).Returns((IDatabase)null);
            _clusterProviderMock.Setup(cp => cp.replayIterator).Returns((IReplayIterator)null);
            _clusterProviderMock.Setup(cp => cp.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" });
            _clusterProviderMock.Setup(cp => cp.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _clusterProviderMock.Setup(cp => cp.replayIterator).Returns((IReplayIterator)null);
            _clusterProviderMock.Setup(cp => cp.CannotStreamAOF).Returns(false);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, FastAofTruncate = false, EnableFastCommit = false });
            _logger = new Mock<ILogger>();
            _replicationManagerInstance = new ReplicationManager();
        }

        [Fact]
        public void ProcessPrimaryStream_ShouldLogErrorAndThrow_WhenReplicaIsRecovering()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockAppendOnlyFile = new Mock<IAppendOnlyFile>();
            var mockReplicationManager = new Mock<IReplicationManager>();
            var mockActiveReplay = new Mock<IActiveReplay>();
            var mockClusterManager = new Mock<IClusterManager>();
            var config = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" };

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.CannotStreamAOF).Returns(true);
            mockClusterProvider.Setup(cp => cp.CurrentConfig).Returns(config);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0 });
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(ao => ao.TailAddress).Returns(100);
            var logger = new Mock<ILogger>();
            var rep = new ReplicationManager
            {
                clusterProvider = mockClusterProvider.Object,
                logger = logger.Object
            };

            byte[] recordBytes = new byte[] { 1, 2, 3 };
            unsafe
            {
                fixed (byte* recordPtr = recordBytes)
                {
                    // Act & Assert
                    var ex = Assert.Throws<GarnetException>(() => rep.ProcessPrimaryStream(recordPtr, recordBytes.Length, 0, 100, 200));
                    Assert.Contains("Replica is recovering cannot sync AOF", ex.Message);
                }
            }

            // Verify that LogError was called
            logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
