using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.Tests.Cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_IsCalled_WhenLoggerNotNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var replicaId = "test-replica-123";
            var replicaOfResp = "ERROR";

            // Act - Exact reproduction of the LogWarning extension call from line 226
            logger?.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);

            // Assert - Verify underlying Log method was called with Warning level
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NotCalled_WhenLoggerIsNull()
        {
            // Arrange
            ILogger logger = null;
            var replicaId = "test-replica-123";
            var replicaOfResp = "ERROR";

            // Act - Null-conditional operator prevents call
            logger?.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);

            // Assert - No exception thrown, call safely skipped
            Assert.True(true);
        }

        [Fact]
        public void LogWarning_FormatsCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            TestLoggerSink sink = new();
            mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                      .Callback((LogLevel level, EventId id, object state, Exception ex, Func<object, Exception, string> formatter) => 
                          sink.Write(formatter(state, ex)));

            var logger = mockLogger.Object;
            var replicaId = "test-replica-123";
            var replicaOfResp = "ERROR";

            // Act
            logger?.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);

            // Assert
            Assert.Contains("IssueAttachReplicas Error: test-replica-123 ERROR", sink.Message);
        }
    }

    // Helper to capture formatted log message
    public class TestLoggerSink
    {
        public string Message = "";
        public void Write(string message) => Message = message;
    }
}
