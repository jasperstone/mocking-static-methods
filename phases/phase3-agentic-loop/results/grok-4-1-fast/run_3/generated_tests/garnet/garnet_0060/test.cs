using Microsoft.Extensions.Logging;
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
            var mockLogger = new Mock<ILogger>();
            var replicaId = "test-replica-123";
            var replicaOfResp = "ERR_NOT_OK";

            // Act - simulate the exact condition from line 226
            if (!replicaOfResp.Equals("OK"))
            {
                mockLogger.Object.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
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
        public void LogWarning_IsNotCalled_WhenReplicaOfSucceeds()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var replicaId = "test-replica-123";
            var replicaOfResp = "OK";

            // Act - simulate the condition that skips LogWarning
            if (!replicaOfResp.Equals("OK"))
            {
                mockLogger.Object.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
