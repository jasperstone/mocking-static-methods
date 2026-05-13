using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsInformationOnStart()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGarnetClient = new Mock<GarnetClientSession>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockAofTaskStore = new Mock<AofTaskStore>();

            var localNodeId = "localNode";
            var remoteNodeId = "remoteNode";
            var startAddress = 100L;

            var aofSyncTaskInfo = new AofSyncTaskInfo(
                mockClusterProvider.Object,
                mockAofTaskStore.Object,
                localNodeId,
                remoteNodeId,
                mockGarnetClient.Object,
                startAddress,
                mockLogger.Object);

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Starting ReplicationManager.ReplicaSyncTask for remote node")),
                    It.Is<object[]>(o => o[0].ToString() == remoteNodeId && o[1].ToString() == startAddress.ToString()),
                    null),
                Times.Once);
        }
    }
}
