using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.MediaEncoding
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task DeleteSegmentFiles_LogsDeletionRange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(fs => fs.GetFilePaths("/tmp/job"))
                .Returns(Array.Empty<string>());
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var jobLoggerMock = new Mock<ILogger<TranscodingJob>>();

            var job = new TranscodingJob(jobLoggerMock.Object)
            {
                Path = "/tmp/job/playlist.m3u8",
                Type = TranscodingJobType.Hls
            };

            var cleaner = new TranscodingSegmentCleaner(
                job,
                loggerMock.Object,
                configMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                segmentLength: 5);

            var deleteSegmentFiles = typeof(TranscodingSegmentCleaner)
                .GetMethod("DeleteSegmentFiles", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(deleteSegmentFiles);

            // Act
            var task = (Task)deleteSegmentFiles!.Invoke(cleaner, new object[] { job, 1L, 3L, 0 })!;
            await task.ConfigureAwait(false);

            // Assert
            fileSystemMock.Verify(fs => fs.GetFilePaths("/tmp/job"), Times.Once);
            fileSystemMock.VerifyNoOtherCalls();

            var logInvocation = Assert.Single(
                loggerMock.Invocations.Where(invocation =>
                    invocation.Method.Name == nameof(ILogger.Log)
                    && invocation.Arguments.Count > 0
                    && invocation.Arguments[0] is LogLevel level
                    && level == LogLevel.Debug));

            Assert.Null(logInvocation.Arguments[3]);

            var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logInvocation.Arguments[2]);
            Assert.Equal("Deleting segment file(s) index {Min} to {Max} from {Path}", state.Single(kv => kv.Key == "{OriginalFormat}").Value);
            Assert.Equal(1L, Convert.ToInt64(state.Single(kv => kv.Key == "Min").Value));
            Assert.Equal(3L, Convert.ToInt64(state.Single(kv => kv.Key == "Max").Value));
            Assert.Equal("/tmp/job/playlist.m3u8", Assert.IsType<string>(state.Single(kv => kv.Key == "Path").Value));

            Assert.Equal("Deleting segment file(s) index 1 to 3 from /tmp/job/playlist.m3u8", state.ToString());
        }
    }
}
