using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverLoggerTests
    {
        [Fact]
        public void LoggerExtensions_LogError_CalledWithCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var slots = new[] { "1", "2", "3" };
            var expectedMessage = "Failed to set local slots {slots} to migrate state";
            var formattedMessage = string.Join(",", slots);

            // Act
            logger?.LogError(expectedMessage, formattedMessage);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
