using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_CalledWithThreeArguments_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;

            // Act - Simulate the exact LogWarning extension call from line 266
            logger.LogWarning(
                "TransmitSlots failed for {cursor} to {current} (with {count} keys)",
                100L, 150L, 5);

            // Assert - Verify the ILogger.Log call was made with correct parameters
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed for {cursor} to {current} (with {count} keys)")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger? logger = null;

            // Act & Assert - The real code uses logger?.LogWarning, so null should be safe
            logger?.LogWarning("Test message {param}", 123);
        }

        [Fact]
        public void LogWarning_WithLongParameters_HandlesCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;

            // Act - Test with the exact parameter types from the source code (long, long, int)
            logger.LogWarning(
                "TransmitSlots failed for {cursor} to {current} (with {count} keys)",
                1000000000000L, 1000000000001L, 1000);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TransmitSlots failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
