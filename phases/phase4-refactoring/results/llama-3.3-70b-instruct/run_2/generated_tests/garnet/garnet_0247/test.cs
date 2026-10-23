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
        var replicationManager = new ReplicationManager(loggerMock.Object); // Assuming ReplicationManager is the class containing the ProcessPrimaryStream method

        // Act and Assert
        try
        {
            replicationManager.ProcessPrimaryStream(new byte[10], 10, 0, 10, 20);
        }
        catch (Exception)
        {
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
