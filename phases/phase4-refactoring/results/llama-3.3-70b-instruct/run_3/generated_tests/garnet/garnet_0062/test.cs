using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task TestLogWarningOnWaitingForAttachToCompleteError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var failoverSession = new FailoverSession(loggerMock.Object);

            // Act and Assert
            await failoverSession.BeginAsyncReplicaFailoverAsync();
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
