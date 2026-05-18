using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;
using System.Threading;
using Xunit;

public class TranscodingSegmentCleanerTests
{
    [Fact]
    public async Task DeleteSegmentFiles_LogsDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TranscodingSegmentCleaner>>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockConfig = new Mock<IConfigurationManager>();
        var mockMediaEncoder = new Mock<IMediaEncoder>();

        var job = new TranscodingJob(mockLogger.Object)
        {
            Path = "/path/to/segments",
            Type = TranscodingJobType.Hls,
            DownloadPositionTicks = TimeSpan.FromSeconds(30).Ticks
        };

        var cleaner = new TranscodingSegmentCleaner(
            job,
            mockLogger.Object,
            mockConfig.Object,
            mockFileSystem.Object,
            mockMediaEncoder.Object,
            segmentLength: 10);

        // Act
        cleaner.Start();

        // Allow some time for the timer to trigger
        await Task.Delay(25000);

        // Assert
        mockLogger.Verify(
            logger => logger.LogDebug(
                It.Is<string>(s => s.Contains("Deleting segment file(s) index 0 to 1 from /path/to/segments")),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<string>()
            ),
            Times.Once
        );
    }
}
