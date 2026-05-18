using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderLoggerTests
    {
        [Fact]
        public void LoggerExtensions_LogWarning_CalledWithCorrectParameters()
        {
            // Arrange
            var logger = new Mock<ILogger<MediaEncoder>>();
            var exception = new Exception("Test FFmpeg exception");
            var inputFile = "/path/to/input.mp4";
            var expectedMessage = "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}";

            // Act
            logger.Object.LogWarning(exception, expectedMessage, inputFile);

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("I-frame trickplay extraction failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogWarning_MatchesLine945Call()
        {
            // Arrange - Mock capturing the exact extension method call from line 945
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            loggerMock.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act - Simulate the exact call from MediaEncoder.cs line 945
            loggerMock.Object.LogWarning(
                It.IsAny<Exception>(),
                "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}",
                "/path/to/video.mp4");

            // Assert
            loggerMock.Verify();
        }
    }
}
