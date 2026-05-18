using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;
using Xunit;
using System.Runtime.CompilerServices; // Add this using directive

[assembly: InternalsVisibleTo("TestProj")] // Ensure this is in the AssemblyInfo.cs of the test project

public class TranscodingSegmentCleanerTests
{
    [Fact]
    public async Task DeleteSegmentFiles_LogsDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TranscodingSegmentCleaner>>();
        var mockJobLogger = new Mock<ILogger<TranscodingJob>>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockConfig = new Mock<IConfigurationManager>();
        var mockMediaEncoder = new Mock<IMediaEncoder>();

        var job = new TranscodingJob(
            mockJobLogger.Object, // Provide the required logger
            "/path/to/segments",
            TranscodingJobType.Hls,
            TimeSpan.FromSeconds(30).Ticks);

        var cleaner = new TranscodingSegmentCleaner(
            job,
            mockLogger.Object,
            mockConfig.Object,
            mockFileSystem.Object,
            mockMediaEncoder.Object,
            segmentLength: 10);

        // Act
        await cleaner.TimerCallback(null);

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
