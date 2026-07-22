using Xunit;
using Moq;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using System;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task TimerCallback_LogsDebugMessage_WhenSegmentDeletionEnabled()
        {
            // Arrange
            var job = new TranscodingJob
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls,
                DownloadPositionTicks = TimeSpan.FromSeconds(1000).Ticks,
                HasExited = false
            };
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            configMock.Setup(x => x.GetEncodingOptions()).Returns(new EncodingOptions
            {
                EnableSegmentDeletion = true,
                SegmentKeepSeconds = 30
            });

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            cleaner.Start();
            await Task.Delay(25000); // Wait for the timer to trigger

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
