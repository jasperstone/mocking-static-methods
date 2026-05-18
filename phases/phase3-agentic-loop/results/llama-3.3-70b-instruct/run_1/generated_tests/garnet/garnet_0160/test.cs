using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void LogError_Called_When_TryAddReplicationTask_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);

            // Act
            aofTaskStore.TryAddReplicationTask("replicaNodeId", 0, out _);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogError_Called_When_TryAddReplicationTask_Truncation_Happens()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(cp => cp.AllowDataLoss).Returns(false);
            var aofTaskStore = new AofTaskStore(clusterProviderMock.Object, logger: loggerMock.Object);
            aofTaskStore.TruncatedUntil = 10;

            // Act
            aofTaskStore.TryAddReplicationTask("replicaNodeId", 0, out _);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Once);
        }
    }
}
