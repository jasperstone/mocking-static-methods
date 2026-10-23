using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task SendCheckpointAsync_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var replicaSyncSession = new ReplicaSyncSession(
            new StoreWrapper(),
            new ClusterProvider(),
            logger: loggerMock.Object);

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }
}
