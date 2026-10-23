using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class ReplicaFailoverSessionTests
{
    [Fact]
    public async Task TestLogWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicaFailoverSession = new Garnet.cluster.FailoverSession(loggerMock.Object);

        // Act
        await replicaFailoverSession.BeginAsyncReplicaFailoverAsync();

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
    }
}
