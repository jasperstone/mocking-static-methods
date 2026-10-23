using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionLoggerTests
    {
        [Fact]
        public void LogCriticalExtension_IsCalledWithExceptionAndMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new InvalidOperationException("Test exception");
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Critical)).Returns(true);

            // Act - Directly test the LogCritical extension method usage pattern
            loggerMock.Object.LogCritical(exception, "IssueAttachReplicas faulted");

            // Assert - Verify underlying Log method was called with correct parameters
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType<object>>(),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType<object>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCriticalExtension_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;
            var exception = new InvalidOperationException("Test exception");

            // Act & Assert - Null-conditional operator ?. prevents the call
            Assert.Same(null, logger);
            logger?.LogCritical(exception, "IssueAttachReplicas faulted");
        }

        [Fact]
        public void LogCriticalExtension_NullLoggerDisabled_DoesNotCallLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Critical)).Returns(false);
            var exception = new InvalidOperationException("Test exception");

            // Act
            loggerMock.Object.LogCritical(exception, "IssueAttachReplicas faulted");

            // Assert - No Log call when logging is disabled
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType<object>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType<object>, Exception?, string>>()),
                Times.Never);
        }
    }
}
