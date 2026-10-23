using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        }

        [Fact]
        public void LogWarning_CalledWithImageNotFoundTemplate_FormatsCorrectly()
        {
            // Arrange
            var imagePath = "/images/missing.jpg";
            var expectedMessage = $"Image not found at {imagePath}";

            // Act - Directly test the LoggerExtensions.LogWarning extension method
            // This matches the exact call at LibraryManager.cs line 2425
            _loggerMock.Object.LogWarning("Image not found at {ImagePath}", imagePath);

            // Assert - Verify the underlying ILogger.Log was called with correct parameters
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at {ImagePath}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((func, t) =>
                    {
                        var formattedMessage = func(It.IsAny<It.IsAnyType>(), null);
                        return formattedMessage == expectedMessage;
                    })),
                Times.Once);
        }

        [Fact]
        public void LogWarning_IgnoresWhenLoggingDisabled()
        {
            // Arrange
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(false);
            var imagePath = "/images/missing.jpg";

            // Act
            _loggerMock.Object.LogWarning("Image not found at {ImagePath}", imagePath);

            // Assert - No Log call when logging is disabled
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogWarning_HandlesNullImagePath()
        {
            // Arrange

            // Act & Assert - Should not throw
            _loggerMock.Object.LogWarning("Image not found at {ImagePath}", (string)null);
            
            _loggerMock.VerifyAll();
        }
    }
}
