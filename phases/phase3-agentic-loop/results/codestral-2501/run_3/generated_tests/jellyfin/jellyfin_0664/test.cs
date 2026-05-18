using System;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Transcoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class TranscodingSegmentCleanerTests
{
    [Fact]
    public async Task DeleteSegmentFiles_LogsDebugMessage()
    {
        // Arrange
        var job = new TranscodingJob
        {
            Path = "test/path",
            Type = TranscodingJobType.Hls,
            DownloadPositionTicks = TimeSpan.FromSeconds(100).Ticks
        };
        var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
        var configMock = new Mock<IConfigurationManager>();
        var fileSystemMock = new Mock<IFileSystem>();
        var mediaEncoderMock = new Mock<IMediaEncoder>();

        configMock.Setup(c => c.GetEncodingOptions()).Returns(new EncodingOptions
        {
            EnableSegmentDeletion = true,
            SegmentKeepSeconds = 50
        });

        var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, configMock.Object, fileSystemMock.Object, mediaEncoderMock.Object, 10);

        // Act
        await cleaner.DeleteSegmentFiles(job, 0, 5, 1500);

        // Assert
        loggerMock.Verify(
            x => x.LogDebug(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting segment file(s) index 0 to 5 from test/path")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
