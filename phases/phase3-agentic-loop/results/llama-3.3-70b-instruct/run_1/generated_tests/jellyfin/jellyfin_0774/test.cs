using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => mediaEncoder.ExtractVideoImagesOnIntervalInternal(
                (enableKeyFrameOnlyExtraction ? "-skip_frame nokey " : string.Empty) + inputArg,
                filterParam,
                vidEncoder,
                threads,
                qualityScale,
                priority,
                cancellationToken));

            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<Exception>(), "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}", inputFile), Times.Once);
        }
    }
}
