using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_Called_When_IssueAttachReplicas_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var failoverSession = new FailoverSession(loggerMock.Object);

            // Act
            await failoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
