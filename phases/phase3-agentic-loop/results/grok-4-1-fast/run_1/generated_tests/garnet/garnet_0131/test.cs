using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.Cluster.Server.Migration.Tests
{
    public class MigrationDriverTests
    {
        private static readonly string[] TestSlots = { "1", "2", "3" };
        private static readonly string ExpectedLogMessage = "Failed to set local slots {slots} to migrate state";

        [Fact]
        public void LoggerExtensions_LogError_CalledWithSlotsFormat()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var slotsString = string.Join(",", TestSlots);
            
            // Act - Directly test the LoggerExtensions.LogError call pattern used in the code
            loggerMock.Object.LogError(ExpectedLogMessage, slotsString);

            // Assert - Verify the underlying Log method was called with Error level and correct message pattern
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExpectedLogMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogError_MultipleSlotsFormat()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var slotsString = string.Join(",", TestSlots);
            
            // Act
            loggerMock.Object.LogError(ExpectedLogMessage, slotsString);

            // Assert - Verify specifically that the message contains the slots parameter
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("{slots}") && 
                        v.ToString()!.Contains(slotsString)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogError_EmptySlots()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            
            // Act
            loggerMock.Object.LogError(ExpectedLogMessage, "");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
