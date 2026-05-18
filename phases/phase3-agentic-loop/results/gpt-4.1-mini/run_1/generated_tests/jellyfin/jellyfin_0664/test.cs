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
        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var jobMock = new Mock<TranscodingJob>();
            jobMock.SetupGet(j => j.Path).Returns("/some/path/file.ts");
            jobMock.SetupGet(j => j.Type).Returns(TranscodingJobType.Hls);

            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            // Setup config to return EncodingOptions with EnableSegmentDeletion true and SegmentKeepSeconds 20
            configMock.Setup(c => c.GetConfiguration<EncodingOptions>("encoding")).Returns(new EncodingOptions
            {
                EnableSegmentDeletion = true,
                SegmentKeepSeconds = 20
            });

            // Setup file system to return empty list for GetFilePaths to avoid exceptions
            fileSystemMock.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(Array.Empty<string>());

            var cleaner = new TranscodingSegmentCleaner(
                jobMock.Object,
                loggerMock.Object,
                configMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                segmentLength: 10);

            // Act
            // Call DeleteSegmentFiles via reflection since it's private
            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            var task = (Task)method.Invoke(cleaner, new object[] { jobMock.Object, 0L, 5L, 1 });
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
}
