using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_Called_When_ReplicaOfResp_Is_Not_OK()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaFailoverSession = new ReplicaFailoverSession(loggerMock.Object);
            var replicaId = "replicaId";
            var replicaOfResp = "Not OK";

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
