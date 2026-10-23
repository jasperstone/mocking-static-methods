using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class AofTaskStoreTests
{
    [Fact]
    public void TryAddReplicationTask_LogsError_WhenStartAddressIsLessThanTruncatedUntil()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProvider = new ClusterProvider(new StoreWrapper()); 
        var aofTaskStore = new AofTaskStore(clusterProvider, logger: loggerMock.Object);
        aofTaskStore.TruncatedUntil = 10;
        var startAddress = 5;

        // Act
        aofTaskStore.TryAddReplicationTask("remoteNodeId", startAddress, out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void TryAddReplicationTask_LogsError_WhenExceptionIsThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProvider = new ClusterProvider(new StoreWrapper()); 
        var aofTaskStore = new AofTaskStore(clusterProvider, logger: loggerMock.Object);
        var startAddress = 10;

        // Act
        aofTaskStore.TryAddReplicationTask("remoteNodeId", startAddress, out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
