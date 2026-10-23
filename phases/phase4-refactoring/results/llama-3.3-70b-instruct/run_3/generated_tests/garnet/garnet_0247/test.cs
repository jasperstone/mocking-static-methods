using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class ReplicationReplicaAofSyncTests
{
    [Fact]
    public void LogWarning_Called_When_Exception_Occurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object);

        // Act and Assert
        try
        {
            replicationReplicaAofSync.ProcessPrimaryStream(new byte[10], 10, 0, 10, 20);
        }
        catch
        {
            // Ignore exception
        }
        loggerMock.Verify(l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
