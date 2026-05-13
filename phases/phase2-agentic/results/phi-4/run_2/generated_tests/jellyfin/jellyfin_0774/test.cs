using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task LogWarningIsCalledOnFfmpegException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockConfigurationManager = new Mock<IServerConfigurationManager>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockBlurayExaminer = new Mock<IBlurayExaminer>();
            var mockLocalization = new Mock<ILocalizationManager>();
            var mockConfig = new Mock<IConfiguration>();
            var mockServerConfig = new Mock<IServerConfigurationManager>();

            var mediaEncoder = new MediaEncoder(
                mockLogger.Object,
                mockConfigurationManager.Object,
                mockFileSystem.Object,
                mockBlurayExaminer.Object,
                mockLocalization.Object,
                mockConfig.Object,
                mockServerConfig.Object);

            var mockEncodingHelper = new Mock<IEncodingHelper>();
            var mockJobState = new Mock<IJobState>();
            var mockCancellationToken = new CancellationToken();

            var options = new EncodingOptions
            {
                HardwareAccelerationType = HardwareAccelerationType.videotoolbox
            };

            var ffmpegException = new FfmpegException("Test exception");
            mockEncodingHelper
                .Setup(eh => eh.GetVideoProcessingFilterParam(It.IsAny<IJobState>(), It.IsAny<EncodingOptions>(), It.IsAny<string>()))
                .Returns("filterParam");

            // Act
            await Assert.ThrowsAsync<FfmpegException>(async () =>
            {
                await mediaEncoder.ExtractVideoImagesOnIntervalInternal(
                    "-skip_frame nokey inputArg",
                    "filterParam",
                    "vidEncoder",
                    1,
                    4,
                    ProcessPriorityClass.Normal,
                    mockCancellationToken);
            });

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("I-frame trickplay extraction failed")),
                    It.Is<Exception>(ex => ex == ffmpegException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
