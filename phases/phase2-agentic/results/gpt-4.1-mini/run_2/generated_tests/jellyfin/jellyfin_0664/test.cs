using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            // Setup config to return options with segment deletion enabled
            var options = new EncodingOptions
            {
                EnableSegmentDeletion = true,
                SegmentKeepSeconds = 20
            };
            configMock.Setup(c => c.GetEncodingOptions()).Returns(options);

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
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Dummy classes to satisfy references
    public class TranscodingJob
    {
        public virtual string? Path { get; set; }
        public virtual TranscodingJobType Type { get; set; }
        public virtual bool HasExited { get; set; }
        public virtual long? DownloadPositionTicks { get; set; }
    }

    public enum TranscodingJobType
    {
        Hls,
        Other
    }

    public class EncodingOptions
    {
        public virtual bool EnableSegmentDeletion { get; set; }
        public virtual int SegmentKeepSeconds { get; set; }
    }

    public interface IConfigurationManager
    {
        EncodingOptions GetEncodingOptions();
    }

    public interface IFileSystem
    {
        IEnumerable<string> GetFilePaths(string path);
        void DeleteFile(string path);
    }

    public interface IMediaEncoder
    {
    }
}
