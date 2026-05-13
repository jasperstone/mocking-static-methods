using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;
using Garnet.cluster;

namespace Garnet.cluster.Server.Replication.ReplicaOps.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_WhenCannotStreamAOF_LogsErrorAndThrows()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 1 });
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" });
            mockReplicationManager.Setup(rm => rm.CannotStreamAOF).Returns(true);

            var replicationManager = new ReplicationManagerTestImpl(mockLogger.Object, mockClusterProvider.Object, mockStoreWrapper.Object)
            {
                logger = mockLogger.Object
            };

            // Act & Assert
            var exception = Assert.Throws<GarnetException>(() => 
                replicationManager.ProcessPrimaryStream((byte*)0, 100, 0, 100, 200));
            
            Assert.Equal("Replica is recovering cannot sync AOF", exception.Message);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t == "Replica is recovering cannot sync AOF"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_WhenDivergentAOFStream_LogsErrorAndThrows()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockAppendOnlyFile = new Mock<IAppendOnlyFile>();
            
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 1 });
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" });
            mockReplicationManager.Setup(rm => rm.CannotStreamAOF).Returns(false);
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(aof => aof.TailAddress).Returns(50L);

            var replicationManager = new ReplicationManagerTestImpl(mockLogger.Object, mockClusterProvider.Object, mockStoreWrapper.Object)
            {
                logger = mockLogger.Object,
                pageSizeBits = 12, // 4KB pages
                ReplicationOffset = 0
            };

            // Act & Assert
            var exception = Assert.Throws<GarnetException>(() => 
                replicationManager.ProcessPrimaryStream((byte*)0, 1000, 0, 100, 200));
            
            Assert.Contains("Divergent AOF Stream", exception.Message);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t!.Contains("Divergent AOF Stream")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_WhenReplicationOffsetMismatchSyncMode_LogsErrorAndThrows()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockAppendOnlyFile = new Mock<IAppendOnlyFile>();
            
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0 }); // sync mode
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" });
            mockReplicationManager.Setup(rm => rm.CannotStreamAOF).Returns(false);
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(aof => aof.TailAddress).Returns(150L);

            var replicationManager = new ReplicationManagerTestImpl(mockLogger.Object, mockClusterProvider.Object, mockStoreWrapper.Object)
            {
                logger = mockLogger.Object,
                pageSizeBits = 12,
                ReplicationOffset = 100 // mismatch with tail 150
            };

            // Act & Assert
            var exception = Assert.Throws<GarnetException>(() => 
                replicationManager.ProcessPrimaryStream((byte*)0, 100, 100, 200, 300));
            
            Assert.Contains("Replication offset mismatch", exception.Message);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t!.Contains("Replication offset mismatch")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }
    }

    // Test implementation with accessible members for testing
    internal class ReplicationManagerTestImpl : ReplicationManager
    {
        public new ILogger<ReplicationManager> logger;
        public new int pageSizeBits;
        public new long ReplicationOffset { get; set; }

        public ReplicationManagerTestImpl(ILogger<ReplicationManager> logger, IClusterProvider clusterProvider, IStoreWrapper storeWrapper)
            : base(logger, clusterProvider, storeWrapper)
        {
            this.logger = logger;
        }

        public new unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
        {
            base.ProcessPrimaryStream(record, recordLength, previousAddress, currentAddress, nextAddress);
        }
    }
}
