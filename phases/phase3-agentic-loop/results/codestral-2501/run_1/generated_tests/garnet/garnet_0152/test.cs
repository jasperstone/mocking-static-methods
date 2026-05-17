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
        private readonly Mock<ClusterProvider> _mockClusterProvider;
        private readonly Mock<AofTaskStore> _mockAofTaskStore;
        private readonly Mock<GarnetClientSession> _mockGarnetClient;
        private readonly Mock<ILogger> _mockLogger;
        private readonly AofSyncTaskInfo _aofSyncTaskInfo;

        public AofSyncTaskInfoTests()
        {
            _mockClusterProvider = new Mock<ClusterProvider>();
            _mockAofTaskStore = new Mock<AofTaskStore>();
            _mockGarnetClient = new Mock<GarnetClientSession>();
            _mockLogger = new Mock<ILogger>();

            _aofSyncTaskInfo = new AofSyncTaskInfo(
                _mockClusterProvider.Object,
                _mockAofTaskStore.Object,
                "localNodeId",
                "remoteNodeId",
                _mockGarnetClient.Object,
                0,
                _mockLogger.Object);
        }

        [Fact]
        public void ReplicaSyncTaskAsync_LogsInformation_WhenStarting()
        {
            // Arrange
            _mockGarnetClient.Setup(client => client.IsConnected).Returns(true);

            // Act
            _aofSyncTaskInfo.ReplicaSyncTaskAsync().Wait();

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask for remote node remoteNodeId starting from address 0")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Consume_LogsWarning_WhenExceptionOccurs()
        {
            // Arrange
            _mockGarnetClient.Setup(client => client.ExecuteClusterAppendLog(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .Throws(new Exception("Test exception"));

            // Act & Assert
            Assert.Throws<Exception>(() => _aofSyncTaskInfo.Consume(IntPtr.Zero, 0, 0, 0, false));
            _mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An exception occurred at ReplicationManager.AofSyncTaskInfo.Consume")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Throttle_ThrowsException_WhenClientIsDisconnected()
        {
            // Arrange
            _mockGarnetClient.Setup(client => client.IsConnected).Returns(false);

            // Act & Assert
            Assert.Throws<GarnetException>(() => _aofSyncTaskInfo.Throttle());
        }

        [Fact]
        public void IsConnected_ReturnsTrue_WhenClientIsConnected()
        {
            // Arrange
            _mockGarnetClient.Setup(client => client.IsConnected).Returns(true);

            // Act
            var isConnected = _aofSyncTaskInfo.IsConnected;

            // Assert
            Assert.True(isConnected);
        }

        [Fact]
        public void IsConnected_ReturnsFalse_WhenClientIsDisconnected()
        {
            // Arrange
            _mockGarnetClient.Setup(client => client.IsConnected).Returns(false);

            // Act
            var isConnected = _aofSyncTaskInfo.IsConnected;

            // Assert
            Assert.False(isConnected);
        }

        [Fact]
        public void StartAddress_ReturnsCorrectValue()
        {
            // Act
            var startAddress = _aofSyncTaskInfo.StartAddress;

            // Assert
            Assert.Equal(0, startAddress);
        }
    }
}
