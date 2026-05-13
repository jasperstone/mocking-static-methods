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
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<GarnetClientSession> _garnetClientMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<AofTaskStore> _aofTaskStoreMock;
        private readonly Mock<TsavoriteLogScanSingleIterator> _iteratorMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<Config> _configMock;
        private readonly Mock<AppendOnlyFileWrapper> _appendOnlyFileMock;
        private readonly Mock<ServerOptions> _serverOptionsMock;

        public AofSyncTaskInfoTests()
        {
            _loggerMock = new Mock<ILogger>();
            _garnetClientMock = new Mock<GarnetClientSession>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _aofTaskStoreMock = new Mock<AofTaskStore>();
            _iteratorMock = new Mock<TsavoriteLogScanSingleIterator>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _configMock = new Mock<Config>();
            _appendOnlyFileMock = new Mock<AppendOnlyFileWrapper>();
            _serverOptionsMock = new Mock<ServerOptions>();

            // Setup clusterProvider to return mock appendOnlyFile
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(new StoreWrapper { appendOnlyFile = _appendOnlyFileMock.Object });
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(_serverOptionsMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(new Config());
        }

        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsStartAndEndsProperly()
        {
            // Arrange
            var remoteNodeId = "node1";
            var startAddress = 100;
            var previousAddress = startAddress;
            var mockGarnetClient = new Mock<GarnetClientSession>();
            mockGarnetClient.Setup(c => c.IsConnected).Returns(true);
            mockGarnetClient.Setup(c => c.Connect());
            mockGarnetClient.Setup(c => c.Dispose());

            var mockIterator = new Mock<TsavoriteLogScanSingleIterator>();
            mockIterator.Setup(i => i.BulkConsumeAllAsync(It.IsAny<IBulkLogEntryConsumer>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

            _clusterProviderMock.Setup(cp => cp.storeWrapper.appendOnlyFile.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, It.IsAny<ILogger>()))
                                .Returns(mockIterator.Object);

            var aofTaskStore = new Mock<AofTaskStore>();
            aofTaskStore.Setup(s => s.TryRemove(It.IsAny<IBulkLogEntryConsumer>())).Returns(true);

            var taskInfo = new AofSyncTaskInfo(
                _clusterProviderMock.Object,
                aofTaskStore.Object,
                "localNode",
                remoteNodeId,
                mockGarnetClient.Object,
                startAddress,
                _loggerMock.Object);

            // Act
            await taskInfo.ReplicaSyncTaskAsync();

            // Assert
            _loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<Itinerary>(), null, It.IsAny<Func<Itinerary, Exception, string>>()), Times.AtLeastOnce);
            mockGarnetClient.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public void Consume_LogsWarningOnException()
        {
            // Arrange
            var payloadPtr = (byte*)0x1234;
            int payloadLength = 10;
            long currentAddress = 200;
            long nextAddress = 300;
            var mockLogger = new Mock<ILogger>();
            var mockClient = new Mock<GarnetClientSession>();
            mockClient.Setup(c => c.ExecuteClusterAppendLog(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                      .Throws(new Exception("Test exception"));

            var taskInfo = new AofSyncTaskInfo(
                _clusterProviderMock.Object,
                _aofTaskStore.Object,
                "localNode",
                "remoteNode",
                mockClient.Object,
                0,
                mockLogger.Object);

            // Act & Assert
            Assert.Throws<Exception>(() => taskInfo.Consume(payloadPtr, payloadLength, currentAddress, nextAddress, true));
            mockLogger.Verify(l => l.LogWarning(It.IsAny<Exception>(), "An exception occurred at ReplicationManager.AofSyncTaskInfo.Consume"), Times.Once);
        }

        [Fact]
        public void Throttle_ThrowsWhenDisconnected()
        {
            // Arrange
            var mockClient = new Mock<GarnetClientSession>();
            mockClient.Setup(c => c.IsConnected).Returns(false);
            var taskInfo = new AofSyncTaskInfo(
                _clusterProviderMock.Object,
                _aofTaskStore.Object,
                "localNode",
                "remoteNode",
                mockClient.Object,
                0,
                _loggerMock.Object);

            // Act & Assert
            Assert.Throws<GarnetException>(() => taskInfo.Throttle());
        }
    }
}
