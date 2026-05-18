using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogCritical_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<GarnetClient>();
            var replicaFailoverSession = new FailoverSession(loggerMock.Object, clientMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => replicaFailoverSession.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[0]));
            loggerMock.Verify(l => l.LogCritical(It.IsAny<Exception>(), "IssueAttachReplicas faulted"), Times.Once);
        }
    }
}
