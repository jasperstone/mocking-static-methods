using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;

public class ReplicaFailoverSessionTests
{
    [Fact]
    public async Task TakeOverAsPrimaryAsync_LogWarning_WhenBeginRecoveryFails()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FailoverSession>>();
        var mockClusterProvider = new Mock<IClusterProvider>();
        var mockReplicationManager = new Mock<IReplicationManager>();
        var mockClusterManager = new Mock<IClusterManager>();
        var mockStoreWrapper = new Mock<IStoreWrapper>();

        mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);

        var failoverSession = new FailoverSession(mockClusterProvider.Object, mockLogger.Object);

        mockReplicationManager.Setup(rm => rm.BeginRecovery(It.IsAny<RecoveryStatus>(), It.IsAny<bool>())).Returns(false);

        // Act
        var result = await failoverSession.TakeOverAsPrimaryAsync();

        // Assert
        mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);

        Assert.False(result);
    }
}
