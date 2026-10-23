using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_Called_When_ReplicaOfResp_Is_Not_OK()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var failoverSession = new FailoverSession(loggerMock.Object);
            var replicaId = "replicaId";
            var replicaOfResp = "Not OK";

            // Act
            await failoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), replicaId, replicaOfResp), Times.Once);
        }
    }
}
