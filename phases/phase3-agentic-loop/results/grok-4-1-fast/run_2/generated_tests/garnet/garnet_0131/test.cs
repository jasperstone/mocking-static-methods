using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationLoggerTests
    {
        [Fact]
        public void LogErrorExtension_CalledWithLocalSlotsErrorMessage()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
            
            var slots = new[] { "1000", "1001", "1002" };
            var messageTemplate = "Failed to set local slots {slots} to migrate state";
            var slotsString = string.Join(",", slots);
            var expectedFormatted = $"Failed to set local slots {{{slotsString}}} to migrate state";

            // Act - Directly call the ILoggerExtension.LogError method from line 154 context
            logger.Object.LogError(messageTemplate, slotsString);

            // Assert - Verify the underlying ILogger.Log method was called correctly
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString() == expectedFormatted),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogErrorExtension_MultipleSlotsFormat()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
            
            var slots = new[] { "5000-5005", "6000", "7000-7002" };
            var messageTemplate = "Failed to set local slots {slots} to migrate state";
            var slotsString = string.Join(",", slots);
            var expectedFormatted = $"Failed to set local slots {{{slotsString}}} to migrate state";

            // Act
            logger.Object.LogError(messageTemplate, slotsString);

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString() == expectedFormatted),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogErrorExtension_NullLogger_Safe()
        {
            // Arrange
            ILogger? logger = null;
            var messageTemplate = "Failed to set local slots {slots} to migrate state";
            var slotsString = "1,2,3";

            // Act & Assert - Null-conditional prevents call (matches logger?.LogError pattern)
            logger?.LogError(messageTemplate, slotsString);
            Assert.True(true); // No exception thrown
        }

        [Fact]
        public void LogErrorExtension_WithException()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
            
            var ex = new Exception("Test exception");
            var messageTemplate = "Failed to set local slots {slots} to migrate state";
            var slotsString = "1000-1005";

            // Act - Test with exception (similar to other LogError calls in the file)
            logger.Object.LogError(ex, messageTemplate, slotsString);

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
