using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogInformation_Called_WithCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var message = "Replica replicaId:{replicaId} requesting checkpoint replicaStoreVersion:{replicaStoreVersion} replicaObjectStoreVersion:{replicaObjectStoreVersion}";
        var replicaId = "replicaId";
        var replicaStoreVersion = 1;
        var replicaObjectStoreVersion = 2;

        // Act
        loggerMock.Object.LogInformation(message, replicaId, replicaStoreVersion, replicaObjectStoreVersion);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<EventId>(), It.IsAny<Exception>(), message, replicaId, replicaStoreVersion, replicaObjectStoreVersion), Times.Once);
    }
}
