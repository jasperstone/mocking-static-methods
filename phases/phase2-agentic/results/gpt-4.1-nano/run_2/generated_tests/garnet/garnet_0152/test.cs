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
        private readonly Mock<Config> _configMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<AppendOnlyFile> _appendOnlyFileMock;
        private readonly Mock<ServerOptions> _serverOptionsMock;

        public AofSyncTaskInfoTests()
        {
            _clusterProviderMock = new Mock<ClusterProvider>();
            _aofTaskStoreMock = new Mock<AofTaskStore>();
            _garnetClientMock = new Mock<GarnetClientSession>();
            _loggerMock = new Mock<ILogger>();
            _iteratorMock = new Mock<TsavoriteLogScanSingleIterator>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _configMock = new Mock<Config>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _appendOnlyFileMock = new Mock<AppendOnlyFile>();
            _serverOptionsMock = new Mock<ServerOptions>();

            // Setup clusterProvider to return mock storeWrapper
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            // Setup clusterProvider to return mock serverOptions
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(_serverOptionsMock.Object);
            // Setup clusterProvider to return mock clusterManager
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            // Setup clusterManager to return mock current config
            var currentConfigMock = new Mock<Config>();
            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);
            // Setup currentConfig to return a dummy address and port
            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 8080));
            // Setup storeWrapper to return mock scan iterator
            _storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _appendOnlyFileMock.Setup(ao => ao.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, It.IsAny<ILogger>()))
                .Returns(_iteratorMock.Object);
            // Setup iterator to have a dummy BulkConsumeAllAsync method
            _iteratorMock.Setup(it => it.BulkConsumeAllAsync(
                It.IsAny<IBulkLogEntryConsumer>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task ReplicaSyncTaskAsync_Should_LogStartingAndTerminateProperly()
        {
            // Arrange
            var remoteNodeId = "node-123";
            var startAddress = 1000L;
            var mockGarnetClient = _garnetClientMock.Object;
            mockGarnetClient.Setup(gc => gc.IsConnected).Returns(true);
            mockGarnetClient.Setup(gc => gc.Dispose());

            var aofTaskStore = new Mock<AofTaskStore>();
            aofTaskStore.Setup(s => s.TryRemove(It.IsAny<AofSyncTaskInfo>())).Returns(true);

            var aofSyncTask = new AofSyncTaskInfo(
                _clusterProviderMock.Object,
                aofTaskStore.Object,
                "local-node",
                remoteNodeId,
                mockGarnetClient,
                startAddress,
                _loggerMock.Object);

            // Act
            await aofSyncTask.ReplicaSyncTaskAsync();

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

        [Fact]
        public async Task ReplicaSyncTaskAsync_Should_LogWarningOnException()
        {
            // Arrange
            var remoteNodeId = "node-456";
            var startAddress = 2000L;
            var mockGarnetClient = _garnetClientMock.Object;
            mockGarnetClient.Setup(gc => gc.IsConnected).Returns(true);
            mockGarnetClient.Setup(gc => gc.Dispose());

            var aofTaskStore = new Mock<AofTaskStore>();
            aofTaskStore.Setup(s => s.TryRemove(It.IsAny<AofSyncTaskInfo>())).Returns(false);

            var aofSyncTask = new AofSyncTaskInfo(
                _clusterProviderMock.Object,
                aofTaskStore.Object,
                "local-node",
                remoteNodeId,
                mockGarnetClient,
                startAddress,
                _loggerMock.Object);

            // Force an exception during the try block
            _storeWrapperMock.Setup(sw => sw.appendOnlyFile.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, It.IsAny<ILogger>()))
                .Throws(new InvalidOperationException("Scan failed"));

            // Act
            await aofSyncTask.ReplicaSyncTaskAsync();

            // Assert
            _loggerMock.Verify(
                log => log.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A exception occurred at ReplicationManager.ReplicaSyncTask")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Consume_Should_CallExecuteClusterAppendLog_And_UpdatePreviousAddress()
        {
            // Arrange
            var aofSyncTask = new AofSyncTaskInfo(
                _clusterProviderMock.Object,
                _aofTaskStoreMock.Object,
                "local-node",
                "remote-node",
                _garnetClientMock.Object,
                1234L,
                _loggerMock.Object);

            var payloadPtr = (byte*)0x1234;
            int payloadLength = 10;
            long currentAddress = 2000;
            long nextAddress = 3000;

            _garnetClientMock.Setup(gc => gc.ExecuteClusterAppendLog(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .Verifiable();

            // Act
            aofSyncTask.Consume(payloadPtr, payloadLength, currentAddress, nextAddress, true);

            // Assert
            _garnetClientMock.Verify(gc => gc.ExecuteClusterAppendLog(
                "local-node", aofSyncTask.previousAddress, currentAddress, nextAddress, (long)payloadPtr, payloadLength), Times.Once);
            Assert.Equal(nextAddress, aofSyncTask.previousAddress);
        }
    }
}
