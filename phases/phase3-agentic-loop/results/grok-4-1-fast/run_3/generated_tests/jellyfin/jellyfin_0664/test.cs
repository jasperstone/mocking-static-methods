using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessageWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            
            var encodingOptions = new EncodingOptions { EnableSegmentDeletion = true };
            configMock.Setup(c => c.GetEncodingOptions()).Returns(encodingOptions);
            
            var job = new TranscodingJob(NullLogger<TranscodingJob>.Instance)
            {
                Path = "/test/path/segment.m3u8",
                Type = TranscodingJobType.Hls
            };

            var cleaner = new TranscodingSegmentCleaner(
                job, loggerMock.Object, configMock.Object, 
                fileSystemMock.Object, mediaEncoderMock.Object, 10);

            var method = typeof(TranscodingSegmentCleaner)
                .GetMethod("DeleteSegmentFiles", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            await (Task)method.Invoke(cleaner, new object[] { job, 1L, 5L, 100 })!;

            // Assert - verify the LogDebug extension method call
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Deleting segment file(s) index 1 to 5 from /test/path/segment.m3u8")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSegmentFiles_ThrowsArgumentException_WhenPathIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            
            var encodingOptions = new EncodingOptions { EnableSegmentDeletion = true };
            configMock.Setup(c => c.GetEncodingOptions()).Returns(encodingOptions);
            
            var job = new TranscodingJob(NullLogger<TranscodingJob>.Instance)
            {
                Path = null
            };

            var cleaner = new TranscodingSegmentCleaner(
                job, loggerMock.Object, configMock.Object, 
                fileSystemMock.Object, mediaEncoderMock.Object, 10);

            var method = typeof(TranscodingSegmentCleaner)
                .GetMethod("DeleteSegmentFiles", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => (Task)method.Invoke(cleaner, new object[] { job, 1L, 5L, 100 })!);
            
            Assert.Equal("Path can't be null.", ex.Message);
        }
    }
}
