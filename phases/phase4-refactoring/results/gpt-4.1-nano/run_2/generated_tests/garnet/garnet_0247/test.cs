using System;
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
        [Fact]
        public void ProcessPrimaryStream_Should_LogWarning_When_NodeIsNotReplica()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockAppendOnlyFile = new Mock<IAppendOnlyFile>();
            var mockDefaultDatabase = new Mock<IDatabase>();
            var mockVectorManager = new Mock<IVectorManager>();
            var mockActiveReplay = new Mock<IActiveReplay>();
            var mockReplayIterator = (object)null;

            // Setup cluster config with role not REPLICA
            var config = new ClusterConfig { LocalNodeRole = NodeRole.MASTER, LocalNodeId = 1 };
            mockClusterManager.Setup(c => c.CurrentConfig).Returns(config);
            mockClusterProvider.Setup(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(c => c.logger).Returns(mockLogger.Object);
            mockClusterProvider.Setup(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.Setup(s => s.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockStoreWrapper.Setup(s => s.DefaultDatabase).Returns(mockDefaultDatabase.Object);
            mockStoreWrapper.Setup(s => s.appendOnlyFile.TailAddress).Returns(100);
            mockStoreWrapper.Setup(s => s.serverOptions).Returns(new ServerOptions { FastAofTruncate = false, ReplOffsetMaxLag = 0, EnableFastCommit = false });
            mockClusterProvider.Setup(c => c.replicationManager).Returns(new ReplicationManager());
            mockClusterProvider.Setup(c => c.activeReplay).Returns(mockActiveReplay.Object);
            mockClusterProvider.Setup(c => c.replayIterator).Returns((object)null);
            mockClusterProvider.Setup(c => c.CannotStreamAOF).Returns(false);
            mockClusterProvider.Setup(c => c.serverOptions).Returns(new ServerOptions { FastAofTruncate = false, ReplOffsetMaxLag = 0, EnableFastCommit = false });
            mockClusterProvider.Setup(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(c => c.clusterManager.CurrentConfig).Returns(config);
            mockClusterProvider.Setup(c => c.storeWrapper.appendOnlyFile.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, It.IsAny<ILogger>())).Returns((object)null);
            mockClusterProvider.Setup(c => c.storeWrapper.appendOnlyFile.SafeInitialize(It.IsAny<long>(), It.IsAny<long>())).Verifiable();

            var replicationManager = new ReplicationManager
            {
                clusterProvider = mockClusterProvider.Object,
                logger = mockLogger.Object,
                activeReplay = mockActiveReplay.Object,
                replayIterator = null,
                storeWrapper = mockStoreWrapper.Object
            };

            byte[] record = new byte[10];
            unsafe
            {
                fixed (byte* pRecord = record)
                {
                    // Act
                    replicationManager.ProcessPrimaryStream(pRecord, record.Length, 0, 0, 0);
                }
            }

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("This node") && v.ToString().Contains("is not a replica")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
