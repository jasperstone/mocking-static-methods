using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_Called_When_WaitingForAttachToComplete_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaFailoverSession = new ReplicaFailoverSession(loggerMock.Object);

            // Act
            await replicaFailoverSession.BeginAsyncReplicaFailoverAsync();

            // Assert
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
