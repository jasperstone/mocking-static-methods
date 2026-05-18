using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Net;
using System;
using System.Runtime.CompilerServices;
using Garnet.common;
using Garnet.server;
using Garnet.client;

[assembly: InternalsVisibleTo("Garnet.cluster.Tests")]

namespace Garnet.cluster.Tests
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTask_StartAddressLessThanTruncatedUntil_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<AofTaskStore>>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockCurrentConfig = new Mock<ClusterConfig>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockServerOptions = new Mock<ServerOptions>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.AllowDataLoss).Returns(false);

            mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockCurrentConfig.Object);
            mockCurrentConfig.Setup(cc => cc.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns((IPAddress.Parse("127.0.0.1"), 7001));
            mockCurrentConfig.Setup(cc => cc.LocalNodeId).Returns("localNodeId");

            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(aof => aof.UnsafeGetLogPageSizeBits()).Returns(10);
            mockAppendOnlyFile.Setup(aof => aof.UnsafeGetReadOnlyAddressLagOffset()).Returns(1024);

            var aofTaskStore = new AofTaskStore(mockClusterProvider.Object, logger: mockLogger.Object);
            aofTaskStore.TruncatedUntil = 100;

            // Act
            var result = aofTaskStore.TryAddReplicationTask("remoteNodeId", 50, out var aofSyncTaskInfo);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("failed to add tasks for AOF sync")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.False(result);
            Assert.Null(aofSyncTaskInfo);
        }
    }
}
