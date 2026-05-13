using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogCritical_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var failoverSession = new FailoverSession(loggerMock.Object);
            var replicaId = "replicaId";
            var configByteArray = new byte[0];
            var clientMock = new Mock<GarnetClient>();
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).Throws(new Exception("Test exception"));
            failoverSession.primaryClient = clientMock.Object;

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            loggerMock.Verify(l => l.LogCritical(It.IsAny<Exception>(), "IssueAttachReplicas faulted"), Times.Once);
        }
    }
}
