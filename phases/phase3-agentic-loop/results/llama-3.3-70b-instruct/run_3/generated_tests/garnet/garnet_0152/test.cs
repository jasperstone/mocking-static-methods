using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Garnet.cluster;

namespace Garnet.cluster.Tests
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
            var localNodeId = "localNodeId";
            var remoteNodeId = "remoteNodeId";
            var startAddress = 0L;

            var aofSyncTaskInfoType = typeof(AofSyncTaskInfo);
            var constructor = aofSyncTaskInfoType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(ClusterProvider), typeof(AofTaskStore), typeof(string), typeof(string), typeof(GarnetClientSession), typeof(long), typeof(ILogger) }, null);
            var aofSyncTaskInfo = constructor.Invoke(new object[] { clusterProviderMock.Object, aofTaskStoreMock.Object, localNodeId, remoteNodeId, garnetClientMock.Object, startAddress, loggerMock.Object });

            // Act
            var replicaSyncTaskAsyncMethod = aofSyncTaskInfoType.GetMethod("ReplicaSyncTaskAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)replicaSyncTaskAsyncMethod.Invoke(aofSyncTaskInfo, null);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {startAddress}"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
