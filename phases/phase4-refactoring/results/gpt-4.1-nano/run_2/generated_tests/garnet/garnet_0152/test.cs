using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.client;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsStartingAndTerminating()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGarnetClient = new Mock<GarnetClientSession>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockConfig = new Mock<ClusterConfig>();
            var mockAofTaskStore = new Mock<AofTaskStore>();

            // Setup cluster provider to return a scan iterator
            var mockIterator = new Mock<TsavoriteLogScanSingleIterator>();
            mockIterator.Setup(i => i.BulkConsumeAllAsync(It.IsAny<IBulkLogEntryConsumer>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockStoreWrapper.Setup(s => s.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(a => a.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, It.IsAny<ILogger>()))
                .Returns(mockIterator.Object);
            mockClusterProvider.Setup(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(c => c.serverOptions.ReplicaSyncDelayMs).Returns(10);
            mockClusterProvider.Setup(c => c.clusterManager.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 1234));
            mockClusterProvider.Setup(c => c.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(c => c.clusterManager.CurrentConfig).Returns(mockConfig.Object);

            var mockAofTaskStore = new Mock<AofTaskStore>();
            mockAofTaskStore.Setup(s => s.TryRemove(It.IsAny<AofSyncTaskInfo>())).Returns(true);

            var garnetClient = new GarnetClientSession();

            var taskInfo = new AofSyncTaskInfo(
                clusterProvider: mockClusterProvider.Object,
                aofTaskStore: mockAofTaskStore.Object,
                localNodeId: "local",
                remoteNodeId: "remote",
                garnetClient: garnetClient,
                startAddress: 0,
                logger: mockLogger.Object);

            // Act
            await taskInfo.ReplicaSyncTaskAsync();

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AofSync task terminated; client disposed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
