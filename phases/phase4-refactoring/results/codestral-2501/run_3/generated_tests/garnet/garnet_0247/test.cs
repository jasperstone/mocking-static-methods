using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicationReplicaAofSyncTests
{
    [Fact]
    public void ProcessPrimaryStream_ExceptionOccurs_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicationReplicaAofSync = new Mock<ReplicationReplicaAofSync>(loggerMock.Object) { CallBase = true };

        // Act
        Action act = () => replicationReplicaAofSync.Object.ProcessPrimaryStream(null, 0, 0, 0, 0);

        // Assert
        var exception = Assert.Throws<GarnetException>(act);
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
