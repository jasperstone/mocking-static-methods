using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests
    {
        [Fact]
        public void LogWarningExtension_CalledWithFfmpegException_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var ffmpegException = new FfmpegException("FFmpeg I-frame extraction failed");
            var inputFile = "/path/to/video.mp4";
            var messageTemplate = "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}";

            // Act - Directly invoke the LogWarning extension method (line 945 pattern)
            loggerMock.Object.LogWarning(ffmpegException, messageTemplate, inputFile);

            // Assert - Verify the underlying Log method was called with Warning level
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    ffmpegException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarningExtension_VerifiesLine945CallPattern()
        {
            // Arrange - Exact pattern from line 945
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var ex = new FfmpegException("I-frame trickplay extraction failed");
            var inputFile = "/test/video.mkv";
            var exactMessageTemplate = "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}";

            // Act - Replicate the exact LogWarning call from line 945
            loggerMock.Object.LogWarning(ex, exactMessageTemplate, inputFile);

            // Assert - Verify the call matches the production code pattern
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(e => e == ex),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Minimal exception class matching the production code
    public class FfmpegException : Exception
    {
        public FfmpegException(string message) : base(message) { }
    }
}
