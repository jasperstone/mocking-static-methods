using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogCritical_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var failoverSession = new FailoverSession(loggerMock.Object);
            var replicaId = "replicaId";
            var configByteArray = new byte[0];

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray));
            loggerMock.Verify(l => l.LogCritical(It.IsAny<Exception>(), "IssueAttachReplicas faulted"), Times.Once);
        }
    }
}
