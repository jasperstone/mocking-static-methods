using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var job = new TranscodingJob
            {
                Path = "/some/path/file.ts",
                Type = TranscodingJobType.Hls
            };

            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            // Setup config to return default EncodingOptions
            configMock.Setup(c => c.GetEncodingOptions()).Returns(new EncodingOptions());

            // Setup file system to return empty list for GetFilePaths
            fileSystemMock.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(new List<string>());

            var segmentLength = 10;
            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, segmentLength);

            // Act
            // Call DeleteSegmentFiles via reflection since it's private
            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            var task = (Task)method.Invoke(cleaner, new object[] { job, 0L, 5L, 1 });
            await task.ConfigureAwait(false);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting segment file(s) index 0 to 5 from /some/path/file.ts")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stubs for dependencies and DTOs
    public class TranscodingJob
    {
        public string? Path { get; set; }
        public TranscodingJobType Type { get; set; }
        public bool HasExited { get; set; }
        public long? DownloadPositionTicks { get; set; }
    }

    public enum TranscodingJobType
    {
        Hls,
        Other
    }

    public class EncodingOptions
    {
        public bool EnableSegmentDeletion { get; set; } = true;
        public int SegmentKeepSeconds { get; set; } = 30;
    }
}
