using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using System.Net;
using System.Reflection;

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
            var mockCurrentConfig = new Mock<CurrentConfig>();
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
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);

            var aofTaskStore = new AofTaskStore(mockClusterProvider.Object, logger: mockLogger.Object);

            // Use reflection to set the TruncatedUntil property
            var truncatedUntilField = typeof(AofTaskStore).GetField("TruncatedUntil", BindingFlags.NonPublic | BindingFlags.Instance);
            truncatedUntilField.SetValue(aofTaskStore, 100);

            var remoteNodeId = "node1";
            var startAddress = 50;
            mockCurrentConfig.Setup(cc => cc.GetWorkerAddressFromNodeId(remoteNodeId)).Returns((IPAddress.Parse("127.0.0.1"), 7001));

            // Act
            var result = aofTaskStore.TryAddReplicationTask(remoteNodeId, startAddress, out var aofSyncTaskInfo);

            // Assert
            Assert.False(result);
            Assert.Null(aofSyncTaskInfo);
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
