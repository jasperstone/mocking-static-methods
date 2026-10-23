using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicasAsync_LogCritical_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaFailoverSession = new ReplicaFailoverSession(loggerMock.Object);

            // Act and Assert
            await replicaFailoverSession.IssueAttachReplicasAsync();
            loggerMock.Verify(l => l.LogCritical(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
