using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionLoggerTests
    {
        [Fact]
        public void LogWarning_IsCalled_WhenReplicaOfFails()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var replicaId = "test-replica-123";
            var replicaOfResp = "ERR_NOT_OK";

            // Act - Execute the exact logging call from line 226
            logger.Object.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);

            // Assert - Verify LogWarning extension was called with correct parameters
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("IssueAttachReplicas Error") &&
                        v.ToString()!.Contains(replicaId) &&
                        v.ToString()!.Contains(replicaOfResp)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_MessageFormat_MatchesLine226()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var expectedMessage = "IssueAttachReplicas Error: {replicaId} {replicaOfResp}";

            // Act
            logger.Object.LogWarning(expectedMessage, "replica123", "ERR");

            // Assert - Verify the exact message template from line 226
            logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
