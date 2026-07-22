using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;

public class ReplicaFailoverSessionTests
{
    [Fact]
    public async Task TakeOverAsPrimaryAsync_LogWarning_Called()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FailoverSession>>();
        var failoverSession = new FailoverSession(mockLogger.Object);

        // Act
        var result = await failoverSession.TakeOverAsPrimaryAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()),
            Times.Once);
    }
}
