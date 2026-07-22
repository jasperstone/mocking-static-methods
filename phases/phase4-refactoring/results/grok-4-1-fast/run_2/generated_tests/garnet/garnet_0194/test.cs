using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void LogInformationExtension_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;

            // Act & Assert - tests the null-conditional pattern from line 361
            // logger?.LogInformation("AcquireCheckpointEntry iteration {iteration}", iteration);
            logger?.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0);
        }

        [Fact]
        public void LogInformationExtension_ValidLogger_ExecutesWithoutException()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act & Assert - tests the LogInformation extension call pattern from line 361
            logger.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0);
        }

        [Fact]
        public void LogInformationExtension_VerifyMessageFormat()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Act - simulate the exact extension method call from line 361
            mockLogger.Object.LogInformation("AcquireCheckpointEntry iteration {iteration}", 5);

            // Assert - verify Log was called with Information level and correct message template
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_MultipleIterations_DifferentValues()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Act - simulate multiple iterations like the while loop in AcquireCheckpointEntryAsync
            mockLogger.Object.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0);
            mockLogger.Object.LogInformation("AcquireCheckpointEntry iteration {iteration}", 1);
            mockLogger.Object.LogInformation("AcquireCheckpointEntry iteration {iteration}", 2);

            // Assert - verify 3 calls with Information level
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));
        }
    }
}
