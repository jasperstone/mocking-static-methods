using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var aofSyncTaskInfo = new AofSyncTaskInfo(
                new ClusterProvider(
                    new StoreWrapper(),
                    new ClusterManager(),
                    new ServerOptions()),
                new AofTaskStore(),
                "localNodeId",
                "remoteNodeId",
                new GarnetClientSession(loggerMock.Object, new Socket()),
                0,
                loggerMock.Object);

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}", "remoteNodeId", 0), Times.Once);
        }

        [Fact]
        public async Task ReplicaSyncTaskAsync_LogWarningCalledOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetClientMock = new Mock<GarnetClientSession>(loggerMock.Object, new Socket());
            var clusterProviderMock = new Mock<ClusterProvider>(new StoreWrapper(), new ClusterManager(), new ServerOptions());
            var aofTaskStoreMock = new Mock<AofTaskStore>();
            var aofSyncTaskInfo = new AofSyncTaskInfo(clusterProviderMock.Object, aofTaskStoreMock.Object, "localNodeId", "remoteNodeId", garnetClientMock.Object, 0, loggerMock.Object);
            garnetClientMock.Setup(g => g.Connect()).Throws(new Exception());

            // Act
            await aofSyncTaskInfo.ReplicaSyncTaskAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), "An exception occurred at ReplicationManager.ReplicaSyncTask - terminating"), Times.Once);
        }
    }
}
