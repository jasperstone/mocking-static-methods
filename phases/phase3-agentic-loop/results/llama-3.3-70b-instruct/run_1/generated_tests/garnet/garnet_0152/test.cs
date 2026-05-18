using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetClientMock = new Mock<GarnetClientSession>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var aofTaskStoreMock = new Mock<AofTaskStore>();
            var localNodeId = "localNodeId";
            var remoteNodeId = "remoteNodeId";
            var startAddress = 0L;
            var aofSyncTaskInfo = new AofSyncTaskInfo(
                clusterProviderMock.Object,
                aofTaskStoreMock.Object,
                localNodeId,
                remoteNodeId,
                garnetClientMock.Object,
                startAddress,
                loggerMock.Object);

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsWarningOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetClientMock = new Mock<GarnetClientSession>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var aofTaskStoreMock = new Mock<AofTaskStore>();
            var localNodeId = "localNodeId";
            var remoteNodeId = "remoteNodeId";
            var startAddress = 0L;
            var aofSyncTaskInfo = new AofSyncTaskInfo(
                clusterProviderMock.Object,
                aofTaskStoreMock.Object,
                localNodeId,
                remoteNodeId,
                garnetClientMock.Object,
                startAddress,
                loggerMock.Object);
            var exception = new Exception("Test exception");

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => aofSyncTaskInfo.ReplicaSyncTaskAsync());
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
