using System;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingSegmentCleanerTests
    {
        private readonly Mock<ILogger<TranscodingSegmentCleaner>> _loggerMock;
        private readonly Mock<TranscodingJob> _jobMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly EncodingOptions _configStub;

        public TranscodingSegmentCleanerTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            _jobMock = new Mock<TranscodingJob>();
            _fileSystemMock = new Mock<IFileSystem>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _configStub = new EncodingOptions();
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage_WhenCalled()
        {
            // Arrange
            _jobMock.Setup(j => j.Path).Returns("/path/to/segments/segment.m3u8");
            
            var cleaner = new TranscodingSegmentCleaner(
                _jobMock.Object,
                _loggerMock.Object,
                new Mock<IConfigurationManager>().Object, // Minimal mock
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                10);

            // Act
            await cleaner.CallDeleteSegmentFilesDirect(_jobMock.Object, 1, 5, 100);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 1L, 5L, "/path/to/segments/segment.m3u8"),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsError_WhenHlsDeleteThrowsException()
        {
            // Arrange
            _jobMock.Setup(j => j.Path).Returns("C:\\");
            _jobMock.Setup(j => j.Type).Returns(TranscodingJobType.Hls);
            
            var cleaner = new TranscodingSegmentCleaner(
                _jobMock.Object,
                _loggerMock.Object,
                new Mock<IConfigurationManager>().Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                10);

            // Act & Assert - Path.GetDirectoryName("C:\\") returns null, causing ArgumentException
            await cleaner.CallDeleteSegmentFilesDirect(_jobMock.Object, 1, 5, 0);
            
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<Exception>(), "Error deleting segment file(s) {Path}", "C:\\"),
                Times.Once);
        }
    }

    // Test helper extension to access private method
    public static class TranscodingSegmentCleanerTestExtensions
    {
        public static Task CallDeleteSegmentFilesDirect(this TranscodingSegmentCleaner cleaner, TranscodingJob job, long idxMin, long idxMax, int delayMs)
        {
            return cleaner.GetType()
                .GetMethod("DeleteSegmentFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(cleaner, new object[] { job, idxMin, idxMax, delayMs }) as Task ?? Task.CompletedTask;
        }
    }
}
