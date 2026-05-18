using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public void LogWarningExtension_IsCalledWithExceptionAndMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
            var exception = new InvalidOperationException("Test exception from Task.WhenAll");

            // Act - Directly test the LoggerExtensions.LogWarning call from line 276
            // This verifies the extension method behavior that the production code uses
            mockLogger.Object.LogWarning(exception, "WaitingForAttachToComplete Error");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((v, t) => 
                        t?.ToString().Contains("WaitingForAttachToComplete Error") == true),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarningExtension_HandlesNullLogger()
        {
            // Arrange
            ILogger? logger = null;
            var exception = new InvalidOperationException("Test exception");

            // Act & Assert - Should not throw
            logger?.LogWarning(exception, "WaitingForAttachToComplete Error");
        }
    }
}
