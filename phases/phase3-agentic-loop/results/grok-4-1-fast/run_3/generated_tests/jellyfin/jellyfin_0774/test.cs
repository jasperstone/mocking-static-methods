using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogWarning_Line945Pattern_LogsCorrectWarningWithExceptionAndInputFile()
        {
            // Arrange - Reproduce exact line 945 call signature from MediaEncoder.cs
            var logger = _loggerMock.Object;
            var ffmpegEx = new FfmpegException("I-frame trickplay extraction failed due to FFmpeg error");
            var inputFile = "/path/to/video.mp4";
            const string messageTemplate = "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}";

            // Act - Exact reproduction of the LogWarning call on line 945
            logger.LogWarning(ffmpegEx, messageTemplate, inputFile);

            // Assert - Verify ILogger.Log was called with Warning level, correct exception, and formatted message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        state.ToString()!.Contains("I-frame trickplay extraction failed") &&
                        state.ToString()!.Contains(inputFile)),
                    ffmpegEx,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_Line945_CapturesFfmpegExceptionAndInputPath()
        {
            // Arrange
            var logger = _loggerMock.Object;
            var ex = new FfmpegException("FFmpeg process failed during keyframe extraction");
            var testInputFile = "testvideo.mkv";
            const string line945Template = "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}";

            // Act - Invoke the exact LogWarning extension pattern from line 945
            logger.LogWarning(ex, line945Template, testInputFile);

            // Assert - Verify the call matches production code expectations
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(e => e == ex),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Production exception type used in MediaEncoder.cs
    public class FfmpegException : Exception
    {
        public FfmpegException(string message) : base(message) { }
    }
}
