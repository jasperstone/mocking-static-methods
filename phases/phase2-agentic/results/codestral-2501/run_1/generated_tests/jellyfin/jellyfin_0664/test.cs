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
            var idxMin = 0;
            var idxMax = 5;
            var delayMs = 1500;

            // Act
            await _cleaner.DeleteSegmentFiles(_job, idxMin, idxMax, delayMs);

            // Assert
            _mockLogger.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting segment file(s) index 0 to 5 from test/path")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }

        [Fact]
        public void DeleteHlsSegmentFiles_LogsDebugMessageForEachFile()
        {
            // Arrange
            var outputFilePath = "test/path/file.m3u8";
            var idxMin = 0;
            var idxMax = 5;
            var files = new List<string> { "test/path/file0.ts", "test/path/file1.ts", "test/path/file2.ts" };
            _mockFileSystem.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(files);

            // Act
            _cleaner.DeleteHlsSegmentFiles(outputFilePath, idxMin, idxMax);

            // Assert
            foreach (var file in files)
            {
                _mockLogger.Verify(
                    x => x.LogDebug(
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting HLS segment file {file}")),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<It.IsAnyType>()),
                    Times.Once);
            }
        }

        [Fact]
        public void DeleteHlsSegmentFiles_ThrowsAggregateExceptionOnError()
        {
            // Arrange
            var outputFilePath = "test/path/file.m3u8";
            var idxMin = 0;
            var idxMax = 5;
            var files = new List<string> { "test/path/file0.ts", "test/path/file1.ts" };
            _mockFileSystem.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(files);
            _mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws(new IOException());

            // Act & Assert
            var ex = Assert.Throws<AggregateException>(() => _cleaner.DeleteHlsSegmentFiles(outputFilePath, idxMin, idxMax));
            Assert.Equal(2, ex.InnerExceptions.Count);
        }
    }
}
