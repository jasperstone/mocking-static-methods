using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
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
            long startAddress = 123;

            // Setup IsConnected to true to avoid calling Connect
            mockGarnetClient.SetupGet(c => c.IsConnected).Returns(true);

            // Setup clusterProvider.storeWrapper.appendOnlyFile.ScanSingle to return a mock iterator
            var mockIterator = new Mock<Tsavorite.core.TsavoriteLogScanSingleIterator>();
            mockIterator.Setup(i => i.BulkConsumeAllAsync(
                It.IsAny<IBulkLogEntryConsumer>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockStoreWrapper = new Mock<dynamic>();
            mockStoreWrapper.Setup(s => s.appendOnlyFile.ScanSingle(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<ILogger>()))
                .Returns(mockIterator.Object);

            mockClusterProvider.SetupGet(c => c.storeWrapper).Returns(mockStoreWrapper.Object);

            // Setup serverOptions.ReplicaSyncDelayMs
            var mockServerOptions = new Mock<dynamic>();
            mockServerOptions.SetupGet(o => o.ReplicaSyncDelayMs).Returns(10);
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(mockServerOptions.Object);

            // Setup clusterManager.CurrentConfig.GetWorkerAddressFromNodeId
            var mockClusterManager = new Mock<dynamic>();
            var mockCurrentConfig = new Mock<dynamic>();
            mockCurrentConfig.Setup(c => c.GetWorkerAddressFromNodeId(remoteNodeId)).Returns(("address", 1234));
            mockClusterManager.SetupGet(m => m.CurrentConfig).Returns(mockCurrentConfig.Object);
            mockClusterProvider.SetupGet(c => c.clusterManager).Returns(mockClusterManager.Object);

            // Setup aofTaskStore.TryRemove to return false to test the LogInformation call in finally
            mockAofTaskStore.Setup(s => s.TryRemove(It.IsAny<AofSyncTaskInfo>())).Returns(false);

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
            // Verify LogInformation call on start
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify LogWarning call on termination
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AofSync task terminated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify LogInformation call for not removing from aofTaskStore
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Did not remove")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
