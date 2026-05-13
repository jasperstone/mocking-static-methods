using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogCritical_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaFailoverSession = new ReplicaFailoverSession(loggerMock.Object);
            var replicaId = "replicaId";
            var configByteArray = new byte[] { 1, 2, 3 };

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => replicaFailoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray));
            loggerMock.Verify(l => l.LogCritical(It.IsAny<Exception>(), "IssueAttachReplicas faulted"), Times.Once);
        }
    }
}
