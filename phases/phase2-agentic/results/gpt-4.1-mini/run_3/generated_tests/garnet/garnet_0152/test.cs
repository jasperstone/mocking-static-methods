using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsInformationOnStart()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGarnetClient = new Mock<Garnet.client.GarnetClientSession>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockAofTaskStore = new Mock<AofTaskStore>();

            var remoteNodeId = "remoteNode";
            var localNodeId = "localNode";
            var startAddress = 123L;

            // Setup IsConnected to false to trigger Connect call
            mockGarnetClient.SetupGet(c => c.IsConnected).Returns(false);
            mockGarnetClient.Setup(c => c.Connect());

            // Setup clusterProvider.storeWrapper.appendOnlyFile.ScanSingle to return a mock iterator
            var mockIterator = new Mock<Tsavorite.core.TsavoriteLogScanSingleIterator>();
            mockIterator.Setup(i => i.BulkConsumeAllAsync(
                It.IsAny<IBulkLogEntryConsumer>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var mockStoreWrapper = new Mock<dynamic>();
            mockStoreWrapper.SetupGet(s => s.appendOnlyFile).Returns(new
            {
                ScanSingle = new Func<long, long, bool, bool, ILogger, Tsavorite.core.TsavoriteLogScanSingleIterator>(
                    (start, max, scanUncommitted, recover, logger) => mockIterator.Object)
            });

            // Setup clusterProvider to return the mock storeWrapper and serverOptions
            var mockServerOptions = new Mock<dynamic>();
            mockServerOptions.SetupGet(o => o.ReplicaSyncDelayMs).Returns(10);

            var mockClusterManager = new Mock<dynamic>();
            var mockCurrentConfig = new Mock<dynamic>();
            mockCurrentConfig.Setup(c => c.GetWorkerAddressFromNodeId(remoteNodeId)).Returns(("127.0.0.1", 1234));
            mockClusterManager.SetupGet(m => m.CurrentConfig).Returns(mockCurrentConfig.Object);

            mockClusterProvider.SetupGet(p => p.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.SetupGet(p => p.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.SetupGet(p => p.clusterManager).Returns(mockClusterManager.Object);

            // Setup aofTaskStore.TryRemove to return true
            mockAofTaskStore.Setup(s => s.TryRemove(It.IsAny<AofSyncTaskInfo>())).Returns(true);

            var aofSyncTaskInfo = new AofSyncTaskInfo(
                mockClusterProvider.Object,
                mockAofTaskStore.Object,
                localNodeId,
                remoteNodeId,
                mockGarnetClient.Object,
                startAddress,
                mockLogger.Object);

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
