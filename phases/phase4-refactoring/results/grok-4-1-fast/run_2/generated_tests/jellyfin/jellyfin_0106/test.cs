using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Library
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger<object>> _mockLogger;

        public LoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger<object>>();
        }

        [Fact]
        public void LogError_WithExceptionAndPath_LogsCorrectMessage()
        {
            // Arrange
            var path = "/test/path/to/video.mp4";
            var exception = new System.InvalidOperationException("Test exception");

            // Act - Exercises the exact LogError extension from line 2129
            _mockLogger.Object.LogError(exception, "Error resolving path {Path}.", path);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<Microsoft.Extensions.Logging.FormattedLogValues>(state => 
                        state.ToString().Contains("Error resolving path /test/path/to/video.mp4")),
                    exception,
                    It.IsAny<System.Func<Microsoft.Extensions.Logging.FormattedLogValues, System.Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WithPathOnly_LogsCorrectMessage()
        {
            // Arrange
            var path = "/test/path/to/video.mp4";

            // Act - Covers the null video case logging
            _mockLogger.Object.LogError("Intro resolver returned null for {Path}.", path);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<Microsoft.Extensions.Logging.FormattedLogValues>(state => 
                        state.ToString().Contains("Intro resolver returned null for /test/path/to/video.mp4")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<System.Func<Microsoft.Extensions.Logging.FormattedLogValues, System.Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_NoParameters_LogsStaticMessage()
        {
            // Act - Covers the null Path and ItemId case
            _mockLogger.Object.LogError("IntroProvider returned an IntroInfo with null Path and ItemId.");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<Microsoft.Extensions.Logging.FormattedLogValues>(state => 
                        state.ToString().Contains("IntroProvider returned an IntroInfo with null Path and ItemId.")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<System.Func<Microsoft.Extensions.Logging.FormattedLogValues, System.Exception, string>>()),
                Times.Once);
        }
    }
}
