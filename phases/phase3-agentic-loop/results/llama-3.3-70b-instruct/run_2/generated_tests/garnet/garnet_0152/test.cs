using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Garnet.cluster
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsInformation()
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
            await (Task)typeof(AofSyncTaskInfo).GetMethod("ReplicaSyncTaskAsync", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(aofSyncTaskInfo, null);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
