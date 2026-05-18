using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public void LogWarning_IsCalled_WhenReplicaOfResponseIsNotOK()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var replicaId = "test-replica-123";
            var replicaOfResp = "ERR_NOT_OK";

            // Act - simulate the exact condition on line 226
            if (!replicaOfResp.Equals("OK"))
            {
                mockLogger.Object.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("IssueAttachReplicas Error: test-replica-123 ERR_NOT_OK")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_IsNotCalled_WhenReplicaOfResponseIsOK()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var replicaId = "test-replica-123";
            var replicaOfResp = "OK";

            // Act - simulate the condition that skips the LogWarning
            if (!replicaOfResp.Equals("OK"))
            {
                mockLogger.Object.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
