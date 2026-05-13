using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task ExtractVideoImagesOnIntervalInternal_LogsWarning_WhenEnableKeyFrameOnlyExtractionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var mediaEncoder = new MediaEncoder(loggerMock.Object, null, null, null, null, null, null);
            var inputFile = "inputFile";
            var inputArg = "inputArg";
            var filterParam = "filterParam";
            var vidEncoder = "vidEncoder";
            var threads = 1;
            var qualityScale = 1;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;
            var enableKeyFrameOnlyExtraction = true;

            // Act
            try
            {
                await mediaEncoder.ExtractVideoImagesOnIntervalInternal(
                    (enableKeyFrameOnlyExtraction ? "-skip_frame nokey " : string.Empty) + inputArg,
                    filterParam,
                    vidEncoder,
                    threads,
                    qualityScale,
                    priority,
                    cancellationToken);
            }
            catch (FfmpegException ex)
            {
                // Assert
                loggerMock.Verify(l => l.LogWarning(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>(), inputFile), Times.Once);
            }
        }
    }
}
