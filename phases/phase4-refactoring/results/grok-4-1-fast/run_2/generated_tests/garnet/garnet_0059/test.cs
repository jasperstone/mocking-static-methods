using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public void LogCritical_VerifyLoggerExtension_CanBeCalled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var logger = mockLogger.Object;
            var exception = new InvalidOperationException("Test exception");

            // Act
            logger.LogCritical(exception, "IssueAttachReplicas faulted");

            // Assert - verify the underlying Log method was called with LogLevel.Critical
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((data, t) => t == "IssueAttachReplicas faulted"),
                    exception,
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCritical_WithNullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger<FailoverSession> logger = null;
            var exception = new InvalidOperationException("Test exception");

            // Act & Assert
            logger?.LogCritical(exception, "IssueAttachReplicas faulted");
        }
    }
}
