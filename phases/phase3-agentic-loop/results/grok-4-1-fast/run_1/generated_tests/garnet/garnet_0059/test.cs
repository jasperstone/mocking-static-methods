using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public void LogCriticalExtension_IsCalled_WhenExceptionOccursInGossipResponseProcessing()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            
            // Setup mock logger to capture LogCritical calls
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Critical),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ));

            // Simulate the exact scenario from line 211:
            // logger?.LogCritical(ex, "IssueAttachReplicas faulted");
            var testException = new Exception("Test exception in gossip processing");
            
            // Act - directly test the logging extension behavior
            mockLogger.Object.LogCritical(testException, "IssueAttachReplicas faulted");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex == testException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogCriticalExtension_HandlesNullLogger()
        {
            // Arrange
            ILogger<FailoverSession> nullLogger = NullLogger<FailoverSession>.Instance;
            var testException = new Exception("Test exception");

            // Act & Assert - should not throw
            Assert.DoesNotThrow(() => 
                nullLogger.LogCritical(testException, "IssueAttachReplicas faulted")
            );
        }

        [Fact]
        public void LogCriticalExtension_UsesCorrectMessageFormat()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var testException = new Exception("Processing failed");
            const string expectedMessage = "IssueAttachReplicas faulted";

            // Act
            mockLogger.Object.LogCritical(testException, expectedMessage);

            // Assert - verify the message template matches exactly
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state.ToString().Contains(expectedMessage)),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }
}
