using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public sealed class TranscodingSegmentCleanerTests : IDisposable
    {
        private readonly Mock<ILogger<TranscodingSegmentCleaner>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly TranscodingJob _job;

        public TranscodingSegmentCleanerTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            _configMock = new Mock<IConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();

            _job = new TranscodingJob(NullLogger<TranscodingJob>.Instance)
            {
                Path = "/path/to/segments/segment.m3u8",
                Type = TranscodingJobType.Hls,
                DownloadPositionTicks = 180000000000L // 180 seconds
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _job?.Dispose();
            }
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessageWithCorrectParameters()
        {
            // Arrange
            var cleaner = new TranscodingSegmentCleaner(
                _job, _loggerMock.Object, _configMock.Object, _fileSystemMock.Object, _mediaEncoderMock.Object, 10);

            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            await (Task)method.Invoke(cleaner, new object[] { _job, 5L, 10L, 100 })!;

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 5L, 10L, "/path/to/segments/segment.m3u8"),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsErrorOnException()
        {
            // Arrange
            _fileSystemMock.Setup(x => x.GetFilePaths(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Test exception"));

            var cleaner = new TranscodingSegmentCleaner(
                _job, _loggerMock.Object, _configMock.Object, _fileSystemMock.Object, _mediaEncoderMock.Object, 10);

            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            await (Task)method.Invoke(cleaner, new object[] { _job, 1L, 5L, 0 })!;

            // Assert - error log should be called due to exception
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<Exception>(), "Error deleting segment file(s) {Path}", "/path/to/segments/segment.m3u8"),
                Times.Once);
        }
    }
}
