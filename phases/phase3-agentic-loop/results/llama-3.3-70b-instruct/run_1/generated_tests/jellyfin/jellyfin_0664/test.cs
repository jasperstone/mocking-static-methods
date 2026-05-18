using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            var configMock = new Mock<IConfigurationManager>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var segmentCleaner = new TranscodingSegmentCleaner(jobMock.Object, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            await ((TranscodingSegmentCleaner)segmentCleaner).DeleteSegmentFiles(jobMock.Object, 0, 10, 1500);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Deleting segment file(s) index {Min} to {Max} from {Path}", 0, 10, "path"), Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessageOnError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var jobMock = new Mock<TranscodingJob>();
            jobMock.SetupGet(j => j.Path).Returns("path");
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IConfigurationManager>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var segmentCleaner = new TranscodingSegmentCleaner(jobMock.Object, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(() => ((TranscodingSegmentCleaner)segmentCleaner).DeleteSegmentFiles(null, 0, 10, 1500));
        }
    }
}
