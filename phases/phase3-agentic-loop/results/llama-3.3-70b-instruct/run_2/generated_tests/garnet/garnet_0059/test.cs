using Xunit;
using Moq;
using System.Threading.Tasks;
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
            var clientMock = new Mock<GarnetClient>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var failoverSession = new FailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Setup mock behavior
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .Throws(new Exception("Test exception"));

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[0]);

            // Assert
            loggerMock.Verify(l => l.LogCritical(It.IsAny<Exception>(), "IssueAttachReplicas faulted"), Times.Once);
        }
    }
}
