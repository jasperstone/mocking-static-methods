using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task ExtractVideoImagesOnIntervalAsync_LogsWarningOnFfmpegException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var mediaEncoder = new MediaEncoder(loggerMock.Object, null, null, null, null, null, null);
            var inputFile = "inputFile";
            var options = new EncodingOptions
            {
                HardwareAccelerationType = HardwareAccelerationType.videotoolbox,
                EnableKeyFrameOnlyExtraction = true,
                VideoEncoder = "videoEncoder",
                Threads = 1,
                QualityScale = 1,
                Priority = ProcessPriorityClass.Normal
            };
            var cancellationToken = new CancellationToken();

            // Act
            await mediaEncoder.ExtractVideoImagesOnIntervalAsync(inputFile, options, cancellationToken);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<Exception>(), "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}", inputFile), Times.Once);
        }
    }
}
