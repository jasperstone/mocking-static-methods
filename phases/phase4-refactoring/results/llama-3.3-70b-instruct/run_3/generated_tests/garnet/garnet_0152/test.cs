using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Garnet.cluster
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
    }
}
