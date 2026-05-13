using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTask_LogsError_WhenTruncationHasHappened()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(cp => cp.AllowDataLoss).Returns(false);
            var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);
            aofTaskStore.TruncatedUntil = 100;
            var startAddress = 50;

            // Act
            aofTaskStore.TryAddReplicationTask("remoteNodeId", startAddress, out _);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}", "TryAddReplicationTasks", startAddress, aofTaskStore.TruncatedUntil), Times.Once);
        }

        [Fact]
        public void TryAddReplicationTask_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);
            var startAddress = 100;
            var remoteNodeId = "remoteNodeId";

            // Act
            try
            {
                aofTaskStore.TryAddReplicationTask(remoteNodeId, startAddress, out _);
            }
            catch (Exception)
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "{method} creating AOF sync task for {replicaNodeId} failed", "TryAddReplicationTasks", remoteNodeId), Times.Once);
        }
    }
}
