using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly TranscodingJob _job;
        private readonly int _segmentLength = 10;

        public TranscodingSegmentCleanerTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            _configMock = new Mock<IConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _job = new TranscodingJob
            {
                Path = "some/path/file.m3u8",
                Type = TranscodingJobType.Hls,
                DownloadPositionTicks = TimeSpan.FromSeconds(120).Ticks
            };
        }

        [Fact]
        public async Task TimerCallback_Should_LogDebug_When_DeletingSegments()
        {
            // Arrange
            var options = new EncodingOptions { EnableSegmentDeletion = true, SegmentKeepSeconds = 20 };
            _configMock.Setup(c => c.GetEncodingOptions()).Returns(options);

            var cleaner = new TranscodingSegmentCleaner(_job, _loggerMock.Object, _configMock.Object, _fileSystemMock.Object, null, _segmentLength);

            // Act
            await cleaner.GetType().GetMethod("TimerCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .InvokeAsync(cleaner, new object[] { null });

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting segment file(s) index 0 to 11 from some/path/file.m3u8")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
