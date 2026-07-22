using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogCritical_Called_When_Exception_Occurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaFailoverSession = new FailoverSession(loggerMock.Object);

            // Act
            await replicaFailoverSession.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[0]);

            // Assert
            loggerMock.Verify(l => l.LogCritical(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
