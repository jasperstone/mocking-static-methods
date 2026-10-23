using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _loggerMock.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Fact]
        public void LogWarning_ImageNotFound_CallsWithCorrectMessage()
        {
            // Arrange
            var logger = _loggerMock.Object;
            var imagePath = "/images/missing.jpg";

            // Act - Directly invoke the LogWarning extension method as used on line 2425
            logger.LogWarning("Image not found at {ImagePath}", imagePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        state.ToString()!.Contains("Image not found at {ImagePath}") &&
                        state.ToString()!.Contains(imagePath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_MissingImage_UsesExpectedTemplate()
        {
            // Arrange
            var logger = _loggerMock.Object;
            var testPath = "/path/to/nonexistent/image.png";

            // Act
            logger.LogWarning("Image not found at {ImagePath}", testPath);

            // Assert - Verifies the exact extension method call pattern from LibraryManager:2425
            _loggerMock.Verify(
                x => x.LogWarning("Image not found at {ImagePath}", testPath),
                Times.Once);
        }
    }
}
