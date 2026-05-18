using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.client;
using System.Threading.Tasks;

public class AofSyncTaskInfoTests
{
    [Fact]
    public async Task ReplicaSyncTaskAsync_LogsInformation_WhenStarting()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>(Mock.Of<StoreWrapper>());
        var aofTaskStoreMock = new Mock<AofTaskStore>(clusterProviderMock.Object, 1, loggerMock.Object);
        var garnetClientMock = new Mock<GarnetClientSession>();
        var remoteNodeId = "remoteNodeId";
        var startAddress = 0L;

        var aofSyncTaskInfo = new AofSyncTaskInfo(
            clusterProviderMock.Object,
            aofTaskStoreMock.Object,
            "localNodeId",
            remoteNodeId,
            garnetClientMock.Object,
            startAddress,
            loggerMock.Object);

        // Act
        await aofSyncTaskInfo.ReplicaSyncTaskAsync();

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask for remote node")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
