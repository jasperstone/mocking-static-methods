using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Configuration;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarningOnFfmpegException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var mediaEncoder = new MediaEncoder(loggerMock.Object, null, null, null, null, null, null);
            var inputFile = "inputFile";
            var options = new EncodingOptions
            {
                HardwareAccelerationType = HardwareAccelerationType.videotoolbox,
                EnableKeyFrameOnlyExtraction = true
            };
            var cancellationToken = new CancellationToken();

            // Act
            await mediaEncoder.ExtractVideoImagesOnInterval(inputFile, options, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
