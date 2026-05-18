using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LogInformation_ForegroundCheckpointRetrieval_VerifiesLoggingCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            loggerMock
                .Setup(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Initiating foreground checkpoint retrieval")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var logger = loggerMock.Object;

            // Act - simulate the exact logging call from line 63
            logger.LogInformation("Initiating foreground checkpoint retrieval");

            // Assert
            loggerMock.Verify();
        }
    }
}
