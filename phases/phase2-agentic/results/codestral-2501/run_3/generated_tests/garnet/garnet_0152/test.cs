using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class AofSyncTaskInfoTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<GarnetClientSession> _garnetClientMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<AofTaskStore> _aofTaskStoreMock;
        private readonly AofSyncTaskInfo _aofSyncTaskInfo;

        public AofSyncTaskInfoTests()
        {
            _loggerMock = new Mock<ILogger>();
            _garnetClientMock = new Mock<GarnetClientSession>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _aofTaskStoreMock = new Mock<AofTaskStore>();

            _aofSyncTaskInfo = new AofSyncTaskInfo(
                _clusterProviderMock.Object,
                _aofTaskStoreMock.Object,
                "localNodeId",
                "remoteNodeId",
                _garnetClientMock.Object,
                0,
                _loggerMock.Object);
        }

        [Fact]
        public void ReplicaSyncTaskAsync_LogsInformation()
        {
            // Arrange
            _garnetClientMock.Setup(client => client.IsConnected).Returns(true);
            _garnetClientMock.Setup(client => client.Connect()).Verifiable();
            _clusterProviderMock.Setup(provider => provider.storeWrapper.appendOnlyFile.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .Returns(new Mock<TsavoriteLogScanSingleIterator>().Object);

            // Act
            _aofSyncTaskInfo.ReplicaSyncTaskAsync().Wait();

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask for remote node remoteNodeId starting from address 0")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void Throttle_ThrowsException_WhenClientDisconnected()
        {
            // Arrange
            _garnetClientMock.Setup(client => client.IsConnected).Returns(false);

            // Act & Assert
            Assert.Throws<GarnetException>(() => _aofSyncTaskInfo.Throttle());
        }

        [Fact]
        public void Consume_LogsWarning_WhenExceptionOccurs()
        {
            // Arrange
            _garnetClientMock.Setup(client => client.ExecuteClusterAppendLog(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .Throws(new Exception("Test exception"));

            // Act & Assert
            Assert.Throws<Exception>(() => _aofSyncTaskInfo.Consume(IntPtr.Zero, 0, 0, 0, false));
            _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An exception occurred at ReplicationManager.AofSyncTaskInfo.Consume")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
