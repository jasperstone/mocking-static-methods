using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLogger
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();
        }

        [Fact]
        public void LogError_Line154Format_WithSlotsParameter_CallsUnderlyingLogCorrectly()
        {
            // Arrange - Exact reproduction of line 154 call:
            // logger?.LogError("Failed to set local slots {slots} to migrate state", string.Join(',', GetSlots));
            var slots = new[] { 1, 2, 3 };
            var slotsString = string.Join(",", slots); // "1,2,3"
            var messageTemplate = "Failed to set local slots {slots} to migrate state";

            // Act - Call the exact LoggerExtensions.LogError used on line 154
            _mockLogger.Object.LogError(messageTemplate, slotsString);

            // Assert - Verify the underlying ILogger.Log was called with correct LogLevel.Error and formatted message
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Failed to set local slots 1,2,3 to migrate state")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_RemoteSlotsFailureFormat_CallsUnderlyingLogCorrectly()
        {
            // Arrange - Tests similar LogError from same method
            var slotsRange = "[1-3]";
            var messageTemplate = "Failed to set remote slots {slots} to import state";

            // Act
            _mockLogger.Object.LogError(messageTemplate, slotsRange);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Failed to set remote slots [1-3] to import state")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_NoParamsFormat_CallsUnderlyingLogCorrectly()
        {
            // Arrange - Tests LogError with no parameters
            var messageTemplate = "Failed to reserve destination vector sets, migration failed";

            // Act
            _mockLogger.Object.LogError(messageTemplate);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Failed to reserve destination vector sets, migration failed")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_SingleWordFormat_CallsUnderlyingLogCorrectly()
        {
            // Arrange - Tests simplest LogError format
            var messageTemplate = "MigrateSlotsDriver failed";

            // Act
            _mockLogger.Object.LogError(messageTemplate);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString() == "MigrateSlotsDriver failed"),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
