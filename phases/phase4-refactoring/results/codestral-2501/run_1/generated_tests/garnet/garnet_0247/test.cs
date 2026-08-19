using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicationReplicaAofSyncTests
{
    [Fact]
    public void LogWarning_Called_When_Exception_Occurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationReplicaAofSync = new ReplicationReplicaAofSync(loggerMock.Object);

        // Act & Assert
        Assert.Throws<GarnetException>(() => replicationReplicaAofSync.ProcessPrimaryStream(IntPtr.Zero, 0, 0, 0, 0));
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
