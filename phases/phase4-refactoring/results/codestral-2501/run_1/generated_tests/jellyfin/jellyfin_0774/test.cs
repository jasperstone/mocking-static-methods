using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dlna;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task ExtractVideoImagesOnInterval_KeyFrameOnlyExtractionFails_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var encodingHelperMock = new Mock<IEncodingHelper>();
            var mediaEncoder = new MediaEncoder(
                loggerMock.Object,
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IBlurayExaminer>(),
                Mock.Of<ILocalizationManager>(),
                Mock.Of<IConfiguration>(),
                Mock.Of<IServerConfigurationManager>()
            );

            var jobState = new JobState();
            var options = new EncodingOptions { HardwareAccelerationType = HardwareAccelerationType.videotoolbox };
            var vidEncoder = "libx264";
            var inputFile = "test.mp4";
            var inputArg = "-i test.mp4";
            var filterParam = "filter";
            var threads = 4;
            var qualityScale = 20;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            encodingHelperMock.Setup(e => e.GetVideoProcessingFilterParam(jobState, options, vidEncoder)).Returns(filterParam);
            mediaEncoder._isLowPriorityHwDecodeSupported = true;

            // Act
            await mediaEncoder.ExtractVideoImagesOnInterval(jobState, options, vidEncoder, inputFile, inputArg, threads, qualityScale, priority, true, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
