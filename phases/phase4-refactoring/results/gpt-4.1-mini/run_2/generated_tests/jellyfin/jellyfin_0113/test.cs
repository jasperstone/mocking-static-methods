using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerLoggerTests
    {
        [Fact]
        public void LogWarning_ImageNotFound_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();

            // Act
            var imagePath = "/path/to/image.jpg";
            loggerMock.Object.LogWarning("Image not found at {ImagePath}", imagePath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Image not found at {imagePath}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
