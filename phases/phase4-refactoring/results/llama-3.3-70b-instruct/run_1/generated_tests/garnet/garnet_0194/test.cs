using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public void LogInformation_Called_WithCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicaSyncSession = new ReplicaSyncSession(
            new StoreWrapper(),
            new ClusterProvider(),
            logger: loggerMock.Object);

        // Act
        replicaSyncSession.AcquireCheckpointEntryAsync().Wait();

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
}
