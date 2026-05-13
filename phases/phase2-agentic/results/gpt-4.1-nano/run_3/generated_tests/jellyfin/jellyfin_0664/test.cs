using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;

namespace MediaBrowser.Tests
{
    public class TranscodingSegmentCleanerTests
    {
        private readonly Mock<ILogger<TranscodingSegmentCleaner>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;

        public TranscodingSegmentCleanerTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            _configMock = new Mock<IConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
        }

        [Fact]
        public async Task TimerCallback_Should_LogDebug_When_Called()
        {
            // Arrange
            var job = new TranscodingJob
            {
                Path = "some/path/file.m3u8",
                Type = TranscodingJobType.Hls,
                DownloadPositionTicks = TimeSpan.FromSeconds(100).Ticks
            };

            var options = new EncodingOptions
            {
                EnableSegmentDeletion = true,
                SegmentKeepSeconds = 20
            };

            _configMock.Setup(c => c.GetEncodingOptions()).Returns(options);

            var cleaner = new TranscodingSegmentCleaner(
                job,
                _loggerMock.Object,
                _configMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                segmentLength: 10);

            // Act
            await cleaner.DeleteSegmentFiles(job, 0, 5, 1500);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting segment file(s) index 0 to 5 from some/path/file.m3u8")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
