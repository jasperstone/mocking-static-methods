using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogInformation_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var aofTaskStoreMock = new Mock<AofTaskStore>();
            var garnetClientMock = new Mock<GarnetClientSession>();
            var cts = new CancellationTokenSource();

            var aofSyncTaskInfo = new AofSyncTaskInfo(
                clusterProviderMock.Object,
                aofTaskStoreMock.Object,
                "localNodeId",
                "remoteNodeId",
                garnetClientMock.Object,
                0,
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
        public async Task ReplicaSyncTaskAsync_LogWarning_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var aofTaskStoreMock = new Mock<AofTaskStore>();
            var garnetClientMock = new Mock<GarnetClientSession>();
            var cts = new CancellationTokenSource();

            var aofSyncTaskInfo = new AofSyncTaskInfo(
                clusterProviderMock.Object,
                aofTaskStoreMock.Object,
                "localNodeId",
                "remoteNodeId",
                garnetClientMock.Object,
                0,
                loggerMock.Object);

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
