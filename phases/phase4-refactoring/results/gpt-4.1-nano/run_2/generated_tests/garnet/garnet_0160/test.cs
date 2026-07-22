using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTask_ShouldLogWarningAndReturnFalse_OnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<ClusterConfig>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockServerOptions = new Mock<ServerOptions>();

            // Setup cluster provider
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.ReplicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(cp => cp.AllowDataLoss).Returns(false);

            // Setup store wrapper
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(af => af.UnsafeGetLogPageSizeBits()).Returns(12);
            mockAppendOnlyFile.Setup(af => af.UnsafeGetReadOnlyAddressLagOffset()).Returns(4096);
            mockAppendOnlyFile.SetupSet(af => af.SafeTailShiftCallback = It.IsAny<Action<long, long>>()).Verifiable();

            // Setup cluster config
            mockConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 7001));
            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockConfig.Object);

            // Setup replication manager
            mockReplicationManager.Setup(rm => rm.GetAofSyncNetworkBufferSettings).Returns((byte)1);
            mockReplicationManager.Setup(rm => rm.GetNetworkPool).Returns((object)null);

            // Create the AofTaskStore
            var store = new AofTaskStore(mockClusterProvider.Object, logger: mockLogger.Object);

            // Setup the cluster config to return local node id
            mockConfig.Setup(c => c.LocalNodeId).Returns("localNode");

            // Act
            // Force the constructor of AofSyncTaskInfo to throw
            // To do this, we can temporarily replace the constructor with a delegate that throws
            // But since we can't do that directly, we simulate an exception during the creation
            // by making the GetWorkerAddressFromNodeId return null
            mockConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns((null, 0));

            // Now, call TryAddReplicationTask and expect it to catch an exception and log warning
            var result = false;
            try
            {
                result = store.TryAddReplicationTask("node1", 0, out var task);
            }
            catch (GarnetException)
            {
                // Expected exception
            }

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("At TryAddReplicationTask task creation")), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
