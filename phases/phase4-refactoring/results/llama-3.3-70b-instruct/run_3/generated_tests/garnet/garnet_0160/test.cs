using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

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
            aofTaskStore.TruncatedUntil = 10;

            // Act
            aofTaskStore.TryAddReplicationTask("remoteNodeId", 5, out _);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
