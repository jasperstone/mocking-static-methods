using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Probing;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task LogWarningIsCalledOnFfmpegExceptionWithKeyFrameOnlyExtraction()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MediaEncoder>>();
            var mockEncodingHelper = new Mock<IEncodingHelper>();
            var mockJobState = new Mock<IJobState>();
            var mockFileSystem = Mock.Of<IFileSystem>();
            var mockBlurayExaminer = Mock.Of<IBlurayExaminer>();
            var mockLocalization = Mock.Of<ILocalizationManager>();
            var mockConfig = Mock.Of<IConfiguration>();
            var mockServerConfig = Mock.Of<IServerConfigurationManager>();

            var mediaEncoder = new MediaEncoder(
                mockLogger.Object,
                mockServerConfig,
                mockFileSystem,
                mockBlurayExaminer,
                mockLocalization,
                mockConfig,
                mockServerConfig);

            var cancellationToken = new CancellationToken();
            var inputFile = "test.mp4";
            var options = new VideoEncodingOptions
            {
                HardwareAccelerationType = HardwareAccelerationType.videotoolbox
            };
            var vidEncoder = "h264_vaapi";
            var threads = 4;
            var qualityScale = 23;
            var priority = ProcessPriorityClass.Normal;
            var enableKeyFrameOnlyExtraction = true;

            // Simulate the condition that leads to the LogWarning call
            var ffmpegException = new FfmpegException("Test exception");
            mockEncodingHelper
                .Setup(e => e.GetVideoProcessingFilterParam(It.IsAny<IJobState>(), It.IsAny<VideoEncodingOptions>(), It.IsAny<string>()))
                .Returns("filterParam");

            // Act
            try
            {
                await mediaEncoder.ExtractVideoImagesOnInterval(
                    inputFile,
                    mockEncodingHelper.Object,
                    mockJobState.Object,
                    options,
                    vidEncoder,
                    threads,
                    qualityScale,
                    priority,
                    enableKeyFrameOnlyExtraction,
                    cancellationToken);
            }
            catch (FfmpegException)
            {
                // Expected exception
            }

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("I-frame trickplay extraction failed")),
                    It.Is<Exception>(ex => ex is FfmpegException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
