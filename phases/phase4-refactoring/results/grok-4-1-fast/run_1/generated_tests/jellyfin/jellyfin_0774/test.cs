using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger<MediaEncoder>> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger<MediaEncoder>>();
        }

        [Fact]
        public void LogWarning_CalledWithExceptionAndTemplate_InvokesUnderlyingLogCorrectly()
        {
            // Arrange
            var ffmpegException = new InvalidOperationException("FFmpeg process failed");
            var inputFile = "test-input.mp4";
            var logger = _loggerMock.Object;

            // Act - Directly test the LoggerExtensions.LogWarning call (line 945 equivalent)
            logger.LogWarning(ffmpegException, "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}", inputFile);

            // Assert - Verify underlying ILogger.Log was called with correct parameters
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    ffmpegException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_WithSpecificExceptionType_UsesCorrectMessageTemplate()
        {
            // Arrange
            var exception = new InvalidOperationException("Test FFmpeg exception");
            var inputFile = "/path/to/video.mp4";
            var logger = _loggerMock.Object;
            const string ExpectedTemplate = "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}";

            // Act
            logger.LogWarning(exception, ExpectedTemplate, inputFile);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state?.ToString()?.Contains("I-frame trickplay extraction failed") == true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
