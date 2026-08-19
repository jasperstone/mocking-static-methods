using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.Controller.Tests
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var job = new TranscodingJob(new LoggerFactory().CreateLogger<TranscodingJob>()) { Path = "path" };
            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, null, null, null, 10);

            // Act
            await cleaner.DeleteSegmentFiles(job, 0, 10, 1500);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 0, 10, "path"), Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_DoesNotThrowException_WhenJobTypeIsNotHls()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var job = new TranscodingJob(new LoggerFactory().CreateLogger<TranscodingJob>()) { Path = "path", Type = TranscodingJobType.Hls };
            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, null, null, null, 10);

            // Act and Assert
            await cleaner.DeleteSegmentFiles(job, 0, 10, 1500);
        }

        [Fact]
        public async Task DeleteSegmentFiles_DoesNotThrowException_WhenJobPathIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var job = new TranscodingJob(new LoggerFactory().CreateLogger<TranscodingJob>()) { Path = null };
            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, null, null, null, 10);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(() => cleaner.DeleteSegmentFiles(job, 0, 10, 1500));
        }
    }
}
