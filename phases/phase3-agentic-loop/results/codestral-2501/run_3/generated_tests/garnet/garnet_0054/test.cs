using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using System.Threading.Tasks;
using System.Reflection;

public class ReplicaFailoverSessionTests
{
    [Fact]
    public async Task TakeOverAsPrimaryAsync_ShouldLogWarning_WhenBeginRecoveryFails()
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

        mockReplicationManager.Setup(rm => rm.BeginRecovery(RecoveryStatus.ClusterFailover, false)).Returns(false);

        // Act
        var methodInfo = typeof(FailoverSession).GetMethod("TakeOverAsPrimaryAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = await (Task<bool>)methodInfo.Invoke(failoverSession, null);

        // Assert
        mockLogger.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);

        Assert.False(result);
    }
}
