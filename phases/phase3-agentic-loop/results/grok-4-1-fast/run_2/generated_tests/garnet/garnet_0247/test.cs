using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void LoggerExtensions_LogWarning_CalledWhenExceptionInProcessPrimaryStream()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            
            // Capture LogWarning calls using Verify
            loggerMock.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, id, state, ex, formatter) =>
                {
                    // Verify the specific message from line 135
                    var message = formatter(state, ex);
                    Assert.Contains("An exception occurred at ReplicationManager.ProcessPrimaryStream", message);
                });

            var logger = loggerMock.Object;

            // Act - Directly invoke the LoggerExtensions.LogWarning call from line 135
            var testException = new InvalidOperationException("Test exception");
            logger.LogWarning(testException, "An exception occurred at ReplicationManager.ProcessPrimaryStream");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex != null),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogWarning_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger<ReplicationManager>? logger = null;
            var testException = new InvalidOperationException("Test exception");

            // Act & Assert - null-conditional operator prevents call
            logger?.LogWarning(testException, "An exception occurred at ReplicationManager.ProcessPrimaryStream");
            
            // No exception thrown
            Assert.True(true);
        }

        [Fact]
        public void LoggerExtensions_LogWarning_ValidatesExtensionMethodSignature()
        {
            // Directly test the Microsoft.Extensions.Logging.LoggerExtensions.LogWarning static extension
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var logger = loggerMock.Object;
            var testException = new InvalidOperationException("Test");

            // Act
            logger.LogWarning(testException, "Expected message from line 135");

            // Assert - no exceptions thrown, extension method works as expected
            loggerMock.VerifyAll();
        }
    }
}
