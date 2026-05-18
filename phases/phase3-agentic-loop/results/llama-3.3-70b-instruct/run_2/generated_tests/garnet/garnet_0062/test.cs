using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_OnWaitingForAttachToCompleteError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaFailoverSession = new FailoverSession(loggerMock.Object);

            // Act
            await replicaFailoverSession.BeginAsyncReplicaFailoverAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), "WaitingForAttachToComplete Error"), Times.Once);
        }
    }
}
