using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingSegmentCleanerTests
    {
        private class TestTranscodingJob : TranscodingJob
        {
            public override string? Path { get; set; }
            public override TranscodingJobType Type { get; set; }
            public override bool HasExited { get; set; }
            public override long? DownloadPositionTicks { get; set; }
        }

        private class TestEncodingOptions : EncodingOptions
        {
            public override bool EnableSegmentDeletion { get; set; }
            public override int SegmentKeepSeconds { get; set; }
        }

        private class TestConfigurationManager : IConfigurationManager
        {
            public EncodingOptions GetEncodingOptions()
            {
                return new EncodingOptions
                {
                    EnableSegmentDeletion = true,
                    SegmentKeepSeconds = 20
                };
            }
        }

        private class TestMediaEncoder : IMediaEncoder { }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var job = new TestTranscodingJob
            {
                Path = "/some/path/file.ts",
                Type = TranscodingJobType.Hls
            };

            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var config = new TestConfigurationManager();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoder = new TestMediaEncoder();

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, config, fileSystemMock.Object, mediaEncoder, 10);

            // Act
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
}
