using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.client;
using System.Threading.Tasks;

public class AofSyncTaskInfoTests
{
    [Fact]
    public void IsConnected_ReturnsTrue_WhenGarnetClientIsConnected()
    {
        // Arrange
        var mockGarnetClient = new Mock<GarnetClientSession>();
        mockGarnetClient.Setup(x => x.IsConnected).Returns(true);
        var aofSyncTaskInfo = new AofSyncTaskInfo(
            null,
            null,
            "localNodeId",
            "remoteNodeId",
            mockGarnetClient.Object,
            0,
            null);

        // Act
        var isConnected = aofSyncTaskInfo.IsConnected;

        // Assert
        Assert.True(isConnected);
    }

    [Fact]
    public void IsConnected_ReturnsFalse_WhenGarnetClientIsNotConnected()
    {
        // Arrange
        var mockGarnetClient = new Mock<GarnetClientSession>();
        mockGarnetClient.Setup(x => x.IsConnected).Returns(false);
        var aofSyncTaskInfo = new AofSyncTaskInfo(
            null,
            null,
            "localNodeId",
            "remoteNodeId",
            mockGarnetClient.Object,
            0,
            null);

        // Act
        var isConnected = aofSyncTaskInfo.IsConnected;

        // Assert
        Assert.False(isConnected);
    }

    [Fact]
    public void Throttle_CallsCompletePendingAndThrottle_WhenGarnetClientIsConnected()
    {
        // Arrange
        var mockGarnetClient = new Mock<GarnetClientSession>();
        mockGarnetClient.Setup(x => x.IsConnected).Returns(true);
        var aofSyncTaskInfo = new AofSyncTaskInfo(
            null,
            null,
            "localNodeId",
            "remoteNodeId",
            mockGarnetClient.Object,
            0,
            null);

        // Act
        aofSyncTaskInfo.Throttle();

        // Assert
        mockGarnetClient.Verify(x => x.CompletePending(false), Times.Once);
        mockGarnetClient.Verify(x => x.Throttle(), Times.Once);
    }

    [Fact]
    public void Throttle_ThrowsException_WhenGarnetClientIsNotConnected()
    {
        // Arrange
        var mockGarnetClient = new Mock<GarnetClientSession>();
        mockGarnetClient.Setup(x => x.IsConnected).Returns(false);
        var aofSyncTaskInfo = new AofSyncTaskInfo(
            null,
            null,
            "localNodeId",
            "remoteNodeId",
            mockGarnetClient.Object,
            0,
            null);

        // Act & Assert
        Assert.Throws<Exception>(() => aofSyncTaskInfo.Throttle());
    }
}
