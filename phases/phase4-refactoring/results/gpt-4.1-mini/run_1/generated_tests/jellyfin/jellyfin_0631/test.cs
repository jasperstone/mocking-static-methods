using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoLoggerTests
    {
        [Fact]
        public void Logger_LogInformation_IsCalled_WithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var video = new VideoTestable(loggerMock.Object);

            var testPath = "testpath";

            // Act
            video.LogOwnedVideoFileNoLongerExists(testPath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Owned video file no longer exists") && v.ToString().Contains(testPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class VideoTestable : Video
        {
            private readonly ILogger _logger;

            public VideoTestable(ILogger logger)
            {
                _logger = logger;
            }

            public override ILogger Logger => _logger;

            public void LogOwnedVideoFileNoLongerExists(string path)
            {
                Logger.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", path);
            }
        }
    }
}
