using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Extensions;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class MediaEncoderTests
{
    [Fact]
    public async Task ExtractVideoImagesOnInterval_LogsWarningOnFfmpegException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MediaEncoder>>();
        var encodingHelperMock = new Mock<IEncodingHelper>(); // Assuming IEncodingHelper is an interface
        var cancellationToken = CancellationToken.None;

        var mediaEncoder = new MediaEncoder(
            loggerMock.Object,
            null, // Mock or replace with actual dependencies
            null,
            null,
            null,
            null,
            null);

        var options = new EncodingOptions
        {
            HardwareAccelerationType = HardwareAccelerationType.videotoolbox
        };

        var jobState = new JobState();
        var inputFile = "test.mp4";
        var inputArg = "-i input.mp4";
        var filterParam = "filter";
        var vidEncoder = "h264";
        var threads = 4;
        var qualityScale = 23;
        var priority = ProcessPriorityClass.Normal;
        var enableKeyFrameOnlyExtraction = true;

        encodingHelperMock
            .Setup(eh => eh.GetVideoProcessingFilterParam(jobState, options, vidEncoder))
            .Returns("filter");

        // Act
        var exception = new FfmpegException("Test exception");
        await Assert.ThrowsAsync<FfmpegException>(() =>
            mediaEncoder.ExtractVideoImagesOnInterval(
                inputArg,
                filterParam,
                vidEncoder,
                threads,
                qualityScale,
                priority,
                cancellationToken,
                options,
                jobState,
                encodingHelperMock.Object,
                enableKeyFrameOnlyExtraction,
                inputFile));

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(
                It.Is<Exception>(ex => ex == exception),
                "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}",
                inputFile),
            Times.Once);
    }
}
