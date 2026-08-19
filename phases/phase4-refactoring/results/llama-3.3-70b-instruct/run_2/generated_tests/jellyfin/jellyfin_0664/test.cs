using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
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
            var configMock = new Mock<MediaBrowser.Model.Configuration.IConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var job = new TranscodingJob { Path = "path" };
            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            await cleaner.DeleteSegmentFiles(job, 0, 10, 1500);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 0, 10, "path"), Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_DoesNotThrowWhenJobPathIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<MediaBrowser.Model.Configuration.IConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var job = new TranscodingJob { Path = null };
            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(() => cleaner.DeleteSegmentFiles(job, 0, 10, 1500));
        }
    }
}
