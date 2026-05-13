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
        public async Task ReplicaSyncTaskAsync_LogsInformationOnStartAndWarningOnTermination()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGarnetClient = new Mock<Garnet.client.GarnetClientSession>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockAofTaskStore = new Mock<AofTaskStore>();
            var mockIter = new Mock<Tsavorite.core.TsavoriteLogScanSingleIterator>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockCurrentConfig = new Mock<CurrentConfig>();

            string localNodeId = "localNode";
            string remoteNodeId = "remoteNode";
            long startAddress = 123;

            // Setup clusterProvider and its properties
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockStoreWrapper.SetupGet(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(aof => aof.ScanSingle(startAddress, long.MaxValue, true, false, mockLogger.Object))
                .Returns(mockIter.Object);

            mockClusterProvider.SetupGet(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockServerOptions.SetupGet(so => so.ReplicaSyncDelayMs).Returns(10);

            mockClusterProvider.SetupGet(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterManager.SetupGet(cm => cm.CurrentConfig).Returns(mockCurrentConfig.Object);

            mockCurrentConfig.Setup(cc => cc.GetWorkerAddressFromNodeId(remoteNodeId))
                .Returns(("127.0.0.1", 8080));

            mockGarnetClient.Setup(gc => gc.IsConnected).Returns(false);
            mockGarnetClient.Setup(gc => gc.Connect());

            mockIter.Setup(i => i.BulkConsumeAllAsync(
                It.IsAny<IBulkLogEntryConsumer>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockAofTaskStore.Setup(store => store.TryRemove(It.IsAny<AofSyncTaskInfo>())).Returns(true);

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

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AofSync task terminated; client disposed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockGarnetClient.Verify(gc => gc.Dispose(), Times.Once);
        }
    }
}
