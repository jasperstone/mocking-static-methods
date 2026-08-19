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
            var job = new TranscodingJob(Mock.Of<ILogger<TranscodingJob>>())
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls
            };
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            await cleaner.DeleteSegmentFiles(job, 0, 1, 100);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting segment file(s) index 0 to 1 from test/path")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_DeletesHlsSegmentFiles()
        {
            // Arrange
            var job = new TranscodingJob(Mock.Of<ILogger<TranscodingJob>>())
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls
            };
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            fileSystemMock.Setup(fs => fs.GetFilePaths(It.IsAny<string>()))
                .Returns(new List<string> { "test/path/segment0.ts", "test/path/segment1.ts" });

            // Act
            await cleaner.DeleteSegmentFiles(job, 0, 1, 100);

            // Assert
            fileSystemMock.Verify(fs => fs.DeleteFile("test/path/segment0.ts"), Times.Once);
            fileSystemMock.Verify(fs => fs.DeleteFile("test/path/segment1.ts"), Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_HandlesException()
        {
            // Arrange
            var job = new TranscodingJob(Mock.Of<ILogger<TranscodingJob>>())
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls
            };
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            fileSystemMock.Setup(fs => fs.GetFilePaths(It.IsAny<string>()))
                .Returns(new List<string> { "test/path/segment0.ts" });
            fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>()))
                .Throws(new IOException("Test exception"));

            // Act
            await cleaner.DeleteSegmentFiles(job, 0, 1, 100);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting segment file(s) test/path")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
