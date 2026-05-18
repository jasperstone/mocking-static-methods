using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var jobMock = new Mock<TranscodingJob>();
            jobMock.SetupGet(j => j.Path).Returns("path");
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var configMock = new Mock<IConfigurationManager>();
            var segmentCleaner = new TranscodingSegmentCleaner(jobMock.Object, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            await segmentCleaner.DeleteSegmentFiles(jobMock.Object, 0, 10, 1500);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 0, 10, "path"), Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_DoesNotThrowWhenJobPathIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var jobMock = new Mock<TranscodingJob>();
            jobMock.SetupGet(j => j.Path).Returns((string?)null);
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var configMock = new Mock<IConfigurationManager>();
            var segmentCleaner = new TranscodingSegmentCleaner(jobMock.Object, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(() => segmentCleaner.DeleteSegmentFiles(jobMock.Object, 0, 10, 1500));
        }

        [Fact]
        public void DeleteHlsSegmentFiles_LogsDebugMessageForEachFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(f => f.GetFilePaths(It.IsAny<string>())).Returns(new[] { "file1", "file2" });
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var configMock = new Mock<IConfigurationManager>();
            var segmentCleaner = new TranscodingSegmentCleaner(new TranscodingJob { Path = "path" }, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            segmentCleaner.DeleteHlsSegmentFiles("path", 0, 10);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Deleting HLS segment file {0}", "file1"), Times.Once);
            loggerMock.Verify(l => l.LogDebug("Deleting HLS segment file {0}", "file2"), Times.Once);
        }
    }
}
