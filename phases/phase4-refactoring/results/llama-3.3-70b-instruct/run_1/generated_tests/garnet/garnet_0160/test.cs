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
            aofTaskStore.TruncatedUntil = 100;
            var startAddress = 50;

            // Act
            aofTaskStore.TryAddReplicationTask("remoteNodeId", startAddress, out _);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
