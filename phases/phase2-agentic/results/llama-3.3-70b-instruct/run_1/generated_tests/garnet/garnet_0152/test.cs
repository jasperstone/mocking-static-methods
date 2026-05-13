using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.common;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var aofTaskStoreMock = new Mock<AofTaskStore>();
            var garnetClientMock = new Mock<GarnetClientSession>();
            var localNodeId = "localNodeId";
            var remoteNodeId = "remoteNodeId";
            var startAddress = 123L;
            var aofSyncTaskInfo = new AofSyncTaskInfo(clusterProviderMock.Object, aofTaskStoreMock.Object, localNodeId, remoteNodeId, garnetClientMock.Object, startAddress, loggerMock.Object);

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<FormattedLogValues>(v => v.ToString().Contains($"Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {startAddress}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.Once);
        }
    }
}
