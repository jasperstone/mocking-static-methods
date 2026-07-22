using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsError_WhenReplicaNodeIdUnknown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var session = new ReplicaSyncSession(
                storeWrapper: null,
                clusterProvider: null,
                replicaNodeId: "unknown",
                logger: mockLogger.Object);

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("PRIMARY-ERR don't know about replicaId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
