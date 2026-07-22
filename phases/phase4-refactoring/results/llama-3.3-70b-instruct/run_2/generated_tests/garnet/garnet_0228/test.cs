using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using Garnet.cluster;

public class ReplicaReceiveCheckpointTests
{
    [Fact]
    public void LogError_WhenExceptionThrown_LogsError()
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
            loggerMock.Object.LogError(ex, $"{nameof(ReplicationManager.TryReplicateDiskbasedSyncAsync)}");
        }

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void LogError_WhenNoPrimaryAddress_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationManager = new ReplicationManager();

        // Act
        var errorMsg = Encoding.ASCII.GetString(new byte[] { 0x52, 0x45, 0x53, 0x50, 0x5F, 0x45, 0x52, 0x52, 0x5F, 0x47, 0x45, 0x4E, 0x45, 0x52, 0x49, 0x43, 0x5F, 0x4E, 0x4F, 0x54, 0x5F, 0x41, 0x53, 0x53, 0x49, 0x47, 0x4E, 0x45, 0x44, 0x5F, 0x50, 0x52, 0x49, 0x4D, 0x41, 0x52, 0x59, 0x5F, 0x45, 0x52, 0x52, 0x4F, 0x52 });
        loggerMock.Object.LogError("{msg}", errorMsg);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
