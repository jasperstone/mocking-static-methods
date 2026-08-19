using System;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingSegmentCleanerTests
    {
        // Derived interface to allow mocking GetEncodingOptions (extension method workaround)
        public interface IConfigurationManagerWithEncodingOptions : IConfigurationManager
        {
            EncodingOptions GetEncodingOptions();
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessage()
        {
            // Arrange
            var job = new TranscodingJob(Mock.Of<ILogger<TranscodingJob>>())
            {
                Path = "/some/path/file.ts",
                Type = TranscodingJobType.Hls
            };

            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManagerWithEncodingOptions>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            configMock.Setup(c => c.GetEncodingOptions()).Returns(new EncodingOptions
            {
                EnableSegmentDeletion = true,
                SegmentKeepSeconds = 20
            });

            fileSystemMock.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(Array.Empty<string>());

            var segmentLength = 10;

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, segmentLength);

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
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugExceptionOnDeleteHlsSegmentFiles()
        {
            // Arrange
            var job = new TranscodingJob(Mock.Of<ILogger<TranscodingJob>>())
            {
                Path = "/some/path/file.ts",
                Type = TranscodingJobType.Hls
            };

            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManagerWithEncodingOptions>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();

            configMock.Setup(c => c.GetEncodingOptions()).Returns(new EncodingOptions
            {
                EnableSegmentDeletion = true,
                SegmentKeepSeconds = 20
            });

            // Setup file system to return one file that will cause DeleteFile to throw IOException
            var filePath = "/some/path/file1.ts";
            fileSystemMock.Setup(fs => fs.GetFilePaths(It.IsAny<string>())).Returns(new[] { filePath });
            fileSystemMock.Setup(fs => fs.DeleteFile(filePath)).Throws(new System.IO.IOException("Delete failed"));

            var segmentLength = 10;

            var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, segmentLength);

            // Act
            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            var task = (Task)method.Invoke(cleaner, new object[] { job, 0L, 5L, 1 });
            await task.ConfigureAwait(false);

            // Assert
            // The first LogDebug call for deleting segment files
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting segment file(s) index 0 to 5 from /some/path/file.ts")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // The LogDebug call for error deleting segment files with exception
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting segment file(s) /some/path/file.ts")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
