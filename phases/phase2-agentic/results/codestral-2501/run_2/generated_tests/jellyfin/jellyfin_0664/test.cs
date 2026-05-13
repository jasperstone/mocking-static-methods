using Xunit;
using Moq;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IO;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingSegmentCleanerTests
    {
        private readonly Mock<ILogger<TranscodingSegmentCleaner>> _mockLogger;
        private readonly Mock<IConfigurationManager> _mockConfig;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IMediaEncoder> _mockMediaEncoder;
        private readonly TranscodingJob _job;
        private readonly TranscodingSegmentCleaner _cleaner;

        public TranscodingSegmentCleanerTests()
        {
            _mockLogger = new Mock<ILogger<TranscodingSegmentCleaner>>();
            _mockConfig = new Mock<IConfigurationManager>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockMediaEncoder = new Mock<IMediaEncoder>();
            _job = new TranscodingJob
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls,
                DownloadPositionTicks = TimeSpan.FromSeconds(1000).Ticks
            };
            _cleaner = new TranscodingSegmentCleaner(_job, _mockLogger.Object, _mockConfig.Object, _mockFileSystem.Object, _mockMediaEncoder.Object, 10);
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var options = new EncodingOptions
            {
                EnableSegmentDeletion = true,
                SegmentKeepSeconds = 100
            };
            _mockConfig.Setup(c => c.GetEncodingOptions()).Returns(options);

            // Act
            await _cleaner.DeleteSegmentFiles(_job, 0, 10, 1500);

            // Assert
            _mockLogger.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteHlsSegmentFiles_DeletesFiles()
        {
            // Arrange
            var files = new[] { "test/path/0.ts", "test/path/1.ts", "test/path/2.ts" };
            _mockFileSystem.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(files);

            // Act
            _cleaner.DeleteHlsSegmentFiles("test/path", 0, 2);

            // Assert
            _mockFileSystem.Verify(fs => fs.DeleteFile(It.IsAny<string>()), Times.Exactly(3));
        }

        [Fact]
        public void DeleteHlsSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var files = new[] { "test/path/0.ts", "test/path/1.ts", "test/path/2.ts" };
            _mockFileSystem.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(files);

            // Act
            _cleaner.DeleteHlsSegmentFiles("test/path", 0, 2);

            // Assert
            _mockLogger.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));
        }
    }
}
