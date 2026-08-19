using System;
using System.IO;
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
    public sealed class TranscodingSegmentCleanerTests : IDisposable
    {
        private readonly Mock<ILogger<TranscodingSegmentCleaner>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly TranscodingJob _job;
        private readonly ILogger<TranscodingJob> _jobLogger;

        public TranscodingSegmentCleanerTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            _configMock = new Mock<IConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _jobLogger = Mock.Of<ILogger<TranscodingJob>>();

            _job = new TranscodingJob(_jobLogger)
            {
                Path = "/path/to/segment.m3u8",
                Type = TranscodingJobType.Hls,
                HasExited = false,
                DownloadPositionTicks = 180000000000L // 180 seconds
            };
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessageWithCorrectParameters()
        {
            // Arrange
            var mockOptions = new Mock<EncodingOptions>();
            mockOptions.Setup(o => o.EnableSegmentDeletion).Returns(true);
            _configMock.Setup(c => c.GetEncodingOptions()).Returns(mockOptions.Object);

            var cleaner = new TranscodingSegmentCleaner(
                _job, _loggerMock.Object, _configMock.Object, _fileSystemMock.Object, _mediaEncoderMock.Object, 10);

            // Make private method accessible via reflection
            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            // Act
            await (Task)method.Invoke(cleaner, new object?[] { _job, 0L, 5L, 100 })!;

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Deleting segment file(s) index 0 to 5 from /path/to/segment.m3u8", StringComparison.Ordinal)),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
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
                _job.Dispose();
            }
        }
    }
}
