using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var job = new TranscodingJob
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls
            };
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IConfigurationManager>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            await cleaner.DeleteSegmentFiles(job, 0, 1, 1500);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 0, 1, "test/path"),
                Times.Once);
        }

        [Fact]
        public void DeleteHlsSegmentFiles_LogsDebugMessageForEachFile()
        {
            // Arrange
            var job = new TranscodingJob
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls
            };
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IConfigurationManager>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            var filesToDelete = new List<string> { "file1.ts", "file2.ts" };
            fileSystemMock.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(filesToDelete);

            // Act
            cleaner.DeleteHlsSegmentFiles("test/path", 0, 1);

            // Assert
            foreach (var file in filesToDelete)
            {
                loggerMock.Verify(
                    x => x.LogDebug("Deleting HLS segment file {0}", file),
                    Times.Once);
            }
        }
    }
}
