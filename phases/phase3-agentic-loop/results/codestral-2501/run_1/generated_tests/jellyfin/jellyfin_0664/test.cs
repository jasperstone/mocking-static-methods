using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly Mock<ILogger<TranscodingSegmentCleaner>> _mockLogger;
        private readonly Mock<IConfigurationManager> _mockConfig;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IMediaEncoder> _mockMediaEncoder;
        private readonly TranscodingSegmentCleaner _cleaner;

        public TranscodingSegmentCleanerTests()
        {
            _mockLogger = new Mock<ILogger<TranscodingSegmentCleaner>>();
            _mockConfig = new Mock<IConfigurationManager>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockMediaEncoder = new Mock<IMediaEncoder>();

            var job = new TranscodingJob
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls,
                DownloadPositionTicks = TimeSpan.FromSeconds(100).Ticks
            };

            _cleaner = new TranscodingSegmentCleaner(job, _mockLogger.Object, _mockConfig.Object, _mockFileSystem.Object, _mockMediaEncoder.Object, 10);
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var job = new TranscodingJob
            {
                Path = "test/path",
                Type = TranscodingJobType.Hls
            };

            // Act
            await _cleaner.DeleteSegmentFiles(job, 0, 1, 1500);

            // Assert
            _mockLogger.Verify(
                x => x.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 0, 1, "test/path"),
                Times.Once);
        }

        [Fact]
        public void DeleteHlsSegmentFiles_DeletesFiles()
        {
            // Arrange
            var files = new List<string> { "test/path/segment0.ts", "test/path/segment1.ts" };
            _mockFileSystem.Setup(x => x.GetFilePaths(It.IsAny<string>())).Returns(files);

            // Act
            _cleaner.DeleteHlsSegmentFiles("test/path/segment.ts", 0, 1);

            // Assert
            _mockFileSystem.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Exactly(2));
            _mockLogger.Verify(
                x => x.LogDebug("Deleting HLS segment file {0}", It.IsAny<string>()),
                Times.Exactly(2));
        }

        [Fact]
        public void DeleteHlsSegmentFiles_LogsErrorOnIOException()
        {
            // Arrange
            var files = new List<string> { "test/path/segment0.ts" };
            _mockFileSystem.Setup(x => x.GetFilePaths(It.IsAny<string>())).Returns(files);
            _mockFileSystem.Setup(x => x.DeleteFile(It.IsAny<string>())).Throws(new IOException());

            // Act
            var ex = Assert.Throws<AggregateException>(() => _cleaner.DeleteHlsSegmentFiles("test/path/segment.ts", 0, 0));

            // Assert
            Assert.IsType<IOException>(ex.InnerExceptions[0]);
            _mockLogger.Verify(
                x => x.LogDebug(It.IsAny<Exception>(), "Error deleting HLS segment file {Path}", It.IsAny<string>()),
                Times.Once);
        }
    }

    public class TranscodingJob
    {
        public string Path { get; set; }
        public TranscodingJobType Type { get; set; }
        public long? DownloadPositionTicks { get; set; }
        public bool HasExited { get; set; }
    }

    public enum TranscodingJobType
    {
        Hls
    }
}
