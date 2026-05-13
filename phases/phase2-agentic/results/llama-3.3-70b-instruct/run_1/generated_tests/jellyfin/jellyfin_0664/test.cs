using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Transcoding;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.Controller.Tests
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var job = new TranscodingJob { Path = "path", Type = TranscodingJobType.Hls, DownloadPositionTicks = 1000 };
            var segmentCleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            await segmentCleaner.DeleteSegmentFiles(job, 0, 10, 1500);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 0, 10, "path"), Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessageOnError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var job = new TranscodingJob { Path = "path", Type = TranscodingJobType.Hls, DownloadPositionTicks = 1000 };
            var segmentCleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);
            fileSystemMock.Setup(f => f.GetFilePaths(It.IsAny<string>())).Throws(new IOException());

            // Act
            await segmentCleaner.DeleteSegmentFiles(job, 0, 10, 1500);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<Exception>(), "Error deleting segment file(s) {Path}", "path"), Times.Once);
        }
    }
}
