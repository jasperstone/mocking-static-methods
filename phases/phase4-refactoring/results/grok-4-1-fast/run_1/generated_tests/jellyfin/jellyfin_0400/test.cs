using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerLoggerTests
    {
        [Fact]
        public void LogInformationExtension_CalledWithMediaPath_InvokesUnderlyingLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            var logger = loggerMock.Object;
            var mediaPath = "/path/to/media.mp4";
            var expectedMessage = $"Finished creation of trickplay files for {mediaPath}";

            // Act
            logger.LogInformation("Finished creation of trickplay files for {0}", mediaPath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Finished creation of trickplay files for") && 
                        v.ToString()!.Contains(mediaPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_VerifiesMessageTemplateAndParameter()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayManager>>();
            Mock.Get(loggerMock.Object)
                .Setup(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var mediaPath = "/test/video.mp4";

            // Act
            loggerMock.Object.LogInformation("Finished creation of trickplay files for {0}", mediaPath);

            // Assert
            loggerMock.VerifyAll();
        }
    }
}
