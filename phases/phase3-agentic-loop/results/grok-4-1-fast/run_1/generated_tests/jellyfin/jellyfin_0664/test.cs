using System;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingSegmentCleanerTests
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

            // Create TranscodingJob with required logger parameter using mock
            var jobLoggerMock = new Mock<ILogger<TranscodingJob>>();
            _job = new TranscodingJob(jobLoggerMock.Object)
            {
                Path = "/path/to/segments/segment.m3u8",
                Type = TranscodingJobType.Hls,
                HasExited = false
            };
        }

        [Fact]
        public async Task DeleteSegmentFiles_ValidParameters_LogsDebugMessage()
        {
            // Arrange
            var cleaner = new TranscodingSegmentCleaner(
                _job,
                _loggerMock.Object,
                _configMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                10);

            var idxMin = 1L;
            var idxMax = 5L;
            var delayMs = 1500;

            // Act
            await CallPrivateMethod(cleaner, _job, idxMin, idxMax, delayMs);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(
                    "Deleting segment file(s) index {Min} to {Max} from {Path}",
                    idxMin,
                    idxMax,
                    _job.Path),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_NullPath_ThrowsArgumentException()
        {
            // Arrange
            _job.Path = null;
            var cleaner = new TranscodingSegmentCleaner(
                _job,
                _loggerMock.Object,
                _configMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                10);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => CallPrivateMethod(cleaner, _job, 1, 5, 1500));
            Assert.Contains("Path can't be null", exception.Message);
        }

        private static async Task CallPrivateMethod(TranscodingSegmentCleaner cleaner, TranscodingJob job, long idxMin, long idxMax, int delayMs)
        {
            var method = typeof(TranscodingSegmentCleaner)
                .GetMethod("DeleteSegmentFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            await (Task)method.Invoke(cleaner, new object[] { job, idxMin, idxMax, delayMs })!;
        }
    }
}
