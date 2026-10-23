using System;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class TranscodingSegmentCleanerTests
{
    [Fact]
    public async Task DeleteSegmentFiles_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
        var job = new TranscodingJob
        {
            Path = "/path/to/segments",
            Type = TranscodingJobType.Hls,
            DownloadPositionTicks = TimeSpan.FromSeconds(30).Ticks
        };
        var cleaner = new TranscodingSegmentCleaner(job, loggerMock.Object, null, null, null, 10);

        // Act
        await cleaner.TimerCallback(null);

        // Assert
        loggerMock.Verify(
            x => x.LogDebug(
                It.Is<string>(s => s.Contains("Deleting segment file(s) index {Min} to {Max} from {Path}")),
                It.Is<long>(min => min == 0),
                It.Is<long>(max => max > 0),
                It.Is<string>(path => path == "/path/to/segments")
            ),
            Times.Once
        );
    }
}
