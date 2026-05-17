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
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<AofTaskStore> _aofTaskStoreMock;
        private readonly Mock<GarnetClientSession> _garnetClientMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<TsavoriteLogScanSingleIterator> _iteratorMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ClusterConfig> _clusterConfigMock;
        private readonly Mock<AppendOnlyFileWrapper> _appendOnlyFileMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<ServerOptions> _serverOptionsMock;

        public AofSyncTaskInfoTests()
        {
            _clusterProviderMock = new Mock<ClusterProvider>();
            _aofTaskStoreMock = new Mock<AofTaskStore>();
            _garnetClientMock = new Mock<GarnetClientSession>();
            _loggerMock = new Mock<ILogger>();
            _iteratorMock = new Mock<TsavoriteLogScanSingleIterator>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _clusterConfigMock = new Mock<ClusterConfig>();
            _appendOnlyFileMock = new Mock<AppendOnlyFileWrapper>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _serverOptionsMock = new Mock<ServerOptions>();
        }

        [Fact]
        public async Task ReplicaSyncTaskAsync_ShouldLogStartAndTerminateProperly()
        {
            // Arrange
            var remoteNodeId = "node1";
            var startAddress = 100L;
            var previousAddress = 100L;
            var mockAofSyncTask = new Mock<AofSyncTaskInfo>(
                _clusterProviderMock.Object,
                _aofTaskStoreMock.Object,
                "localNode",
                remoteNodeId,
                _garnetClientMock.Object,
                startAddress,
                _loggerMock.Object);

            mockAofSyncTask.SetupGet(x => x.IsConnected).Returns(true);
            mockAofSyncTask.Object.previousAddress = previousAddress;

            // Setup clusterProvider to return a mock iterator
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _appendOnlyFileMock.Setup(ao => ao.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, It.IsAny<ILogger>()))
                .Returns(_iteratorMock.Object);

            // Setup iterator to simulate async consumption
            _iteratorMock.Setup(it => it.BulkConsumeAllAsync(
                It.IsAny<IBulkLogEntryConsumer>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Setup clusterProvider.clusterManager.CurrentConfig.GetWorkerAddressFromNodeId
            var mockAddress = ("127.0.0.1", 8080);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(_clusterConfigMock.Object);
            _clusterConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(remoteNodeId))
                .Returns(mockAddress);

            // Act
            await mockAofSyncTask.Object.ReplicaSyncTaskAsync();

            // Assert
            _loggerMock.Verify(
                log => log.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                log => log.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AofSync task terminated; client disposed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
