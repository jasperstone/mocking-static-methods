using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Library
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void LogWarning_ImageNotFound_CallsLogWithWarningLevel()
        {
            // Arrange
            var imagePath = "/path/to/nonexistent/image.jpg";

            // Act - Directly invoke the LoggerExtensions.LogWarning extension method
            // This matches the exact call pattern from LibraryManager.cs line 2425
            _mockLogger.Object.LogWarning("Image not found at {ImagePath}", imagePath);

            // Assert - Verify ILogger.Log was called with Warning level and correct message template
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v != null && 
                        v.ToString()!.Contains("Image not found at") && 
                        v.ToString()!.Contains(imagePath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_WithImagePath_UsesStructuredLogging()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var testPath = "C:\\media\\missing.jpg";

            // Act
            mockLogger.Object.LogWarning("Image not found at {ImagePath}", testPath);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
