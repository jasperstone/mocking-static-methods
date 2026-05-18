using System;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
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
            
            var options = new EncodingOptions { EnableSegmentDeletion = true };
            configMock.Setup(c => c.GetEncodingOptions()).Returns(options);

            var job = new TranscodingJob(NullLogger<TranscodingJob>.Instance)
            {
                Path = "/test/path/segment.m3u8",
                Type = TranscodingJobType.Hls
            };

            var cleaner = new TranscodingSegmentCleaner(
                job, loggerMock.Object, configMock.Object, 
                fileSystemMock.Object, mediaEncoderMock.Object, 10);

            var idxMin = 1L;
            var idxMax = 5L;
            var expectedMessage = $"Deleting segment file(s) index {idxMin} to {idxMax} from {job.Path}";

            loggerMock
                .Setup(x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Use reflection to call private method
            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            
            // Act
            await (Task)method.Invoke(cleaner, new object[] { job, idxMin, idxMax, 100 })!;

            // Assert
            loggerMock.Verify();
        }

        [Fact]
        public async Task DeleteSegmentFiles_ThrowsArgumentException_WhenPathIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            var configMock = new Mock<IConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            
            var job = new TranscodingJob(NullLogger<TranscodingJob>.Instance)
            {
                Path = null
            };

            var cleaner = new TranscodingSegmentCleaner(
                job, loggerMock.Object, configMock.Object, 
                fileSystemMock.Object, mediaEncoderMock.Object, 10);

            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => 
            {
                await (Task)method.Invoke(cleaner, new object[] { job, 1L, 5L, 100 })!;
            });

            Assert.Equal("Path can't be null.", exception.Message);
        }
    }
}
