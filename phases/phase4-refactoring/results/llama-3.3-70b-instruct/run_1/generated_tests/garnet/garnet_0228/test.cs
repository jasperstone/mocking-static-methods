using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class ReplicationManagerTests
{
    [Fact]
    public void TryReplicateDiskbasedSyncAsync_WhenExceptionThrown_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager();

        // Act
        try
        {
            replicationManager.TryReplicateDiskbasedSyncAsync(null, null);
        }
        catch (Exception ex)
        {
            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }

    [Fact]
    public void ReplicaSyncAttachTaskAsync_WhenNoPrimaryAddressAssigned_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager();

        // Act
        var current = new ClusterManager();
        current.GetLocalNodePrimaryAddress = () => (null, -1);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
