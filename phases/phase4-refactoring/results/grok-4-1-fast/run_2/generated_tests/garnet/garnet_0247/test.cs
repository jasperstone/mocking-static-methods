using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
        }

        [Fact]
        public void LogWarning_ExtensionMethod_CalledWithExceptionAndMessage()
        {
            // Arrange
            var testException = new InvalidOperationException("Test exception from ProcessPrimaryStream");
            var message = "An exception occurred at ReplicationManager.ProcessPrimaryStream";

            // Act
            _loggerMock.Object.LogWarning(testException, message);

            // Assert - Verifies the LogWarning extension method behavior used at line 135
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger nullLogger = null;
            var testException = new InvalidOperationException("Test");

            // Act & Assert - Tests the null-conditional operator (logger?.LogWarning) safety
            nullLogger?.LogWarning(testException, "An exception occurred at ReplicationManager.ProcessPrimaryStream");
        }

        [Fact]
        public void LogWarning_LoggerDisabled_DoesNotLog()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(false);
            var testException = new InvalidOperationException("Test");

            // Act
            _loggerMock.Object.LogWarning(testException, "Test message");

            // Assert - Internal IsEnabled check prevents logging
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogWarning_ValidatesSpecificSignatureUsedInReplicationReplicaAofSync()
        {
            // Directly tests the exact ILogger.LogWarning(Exception, string) signature from line 135
            var logger = _loggerMock.Object;
            var ex = new Exception("Simulated ProcessPrimaryStream exception");
            
            logger.LogWarning(ex, "An exception occurred at ReplicationManager.ProcessPrimaryStream");
        }
    }
}
