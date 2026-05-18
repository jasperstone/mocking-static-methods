using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_CalledWithExceptionAndMessage_ExecutesCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            Exception testException = new InvalidOperationException("GOSSIP round faulted test");

            // Act - Directly invoke the extension method pattern that would be called on line 252
            // logger?.LogWarning(task.Exception, "GOSSIP round faulted");
            if (mockLogger.Object is ILogger logger)
            {
                LoggerExtensions.LogWarning(logger, testException, "GOSSIP round faulted");
            }

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => (state?.ToString() ?? "").Contains("GOSSIP round faulted")),
                It.Is<Exception>(ex => ex == testException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NullLogger_DoesNotThrow()
        {
            // Act & Assert
            ILogger? nullLogger = null;
            Exception testException = new InvalidOperationException("test");
            // This should not throw (null-conditional operator)
            nullLogger?.LogWarning(testException, "GOSSIP round faulted");
        }

        [Fact]
        public void LogWarning_NullException_DoesNotThrow()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            mockLogger.Object.LogWarning(null, "GOSSIP round faulted");

            // Assert - No exception thrown, logger called with null exception
            mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
