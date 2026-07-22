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
            var job = new Mock<TranscodingJob>();
            job.Setup(j => j.Path).Returns("test/path");
            job.Setup(j => j.Type).Returns(TranscodingJobType.Hls);

            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            var cleaner = new TranscodingSegmentCleaner(job.Object, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

            // Act
            await cleaner.DeleteSegmentFiles(job.Object, 0, 1, 1500);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock TranscodingJob class
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
