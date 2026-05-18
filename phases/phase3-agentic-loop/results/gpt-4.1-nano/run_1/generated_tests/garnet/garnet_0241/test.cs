using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public unsafe void ProcessPrimaryStream_ShouldLogError_WhenCannotStreamAOF()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationReplicaAofSync>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockDefaultDatabase = new Mock<Database>();
            var mockVectorManager = new Mock<VectorManager>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node-1" };

            // Setup clusterProvider
            mockClusterProvider.Setup(cp => cp.logger).Returns(mockLogger.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockConfig);
            mockClusterProvider.Setup(cp => cp.serverOptions.ReplicationOffsetMaxLag).Returns(0);
            mockClusterProvider.Setup(cp => cp.storeWrapper.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper.DefaultDatabase).Returns(mockDefaultDatabase.Object);
            mockDefaultDatabase.Setup(db => db.VectorManager).Returns(mockVectorManager.Object);
            mockVectorManager.Setup(vm => vm.WaitForVectorOperationsToComplete());

            // Setup AppendOnlyFile
            long tailAddress = 100;
            mockAppendOnlyFile.Setup(af => af.TailAddress).Returns(tailAddress);
            mockAppendOnlyFile.Setup(af => af.SafeInitialize(It.IsAny<long>(), It.IsAny<long>()));
            mockAppendOnlyFile.Setup(af => af.UnsafeEnqueueRaw(It.IsAny<Span<byte>>(), It.IsAny<bool>())).Returns(true);

            // Setup clusterManager
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockConfig);

            // Setup replicationManager to simulate CannotStreamAOF
            mockReplicationManager.Setup(rm => rm.CannotStreamAOF).Returns(true);

            // Instantiate the class under test
            var sync = new ReplicationReplicaAofSync
            {
                logger = mockLogger.Object,
                clusterProvider = mockClusterProvider.Object,
                pageSizeBits = 12,
                ReplicationOffset = 50,
                storeWrapper = mockStoreWrapper.Object,
                clusterManager = mockClusterManager.Object,
                activeReplay = new Lockable<bool>(true),
                replayIterator = null
            };

            // Prepare dummy record
            byte[] recordBytes = new byte[10];
            fixed (byte* recordPtr = recordBytes)
            {
                // Act
                var exception = Record.Exception(() =>
                    sync.ProcessPrimaryStream(recordPtr, recordBytes.Length, 0, 60, 70));

                // Assert
                Assert.NotNull(exception);
                Assert.IsType<GarnetException>(exception);
                var garnetEx = (GarnetException)exception;
                Assert.Contains("Replica is recovering cannot sync AOF", garnetEx.Message);
                mockLogger.Verify(
                    logger => logger.Log(
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
