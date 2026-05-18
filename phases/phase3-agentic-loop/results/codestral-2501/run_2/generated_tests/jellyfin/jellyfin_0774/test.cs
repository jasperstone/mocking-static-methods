using Xunit;
using Moq;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common;
using MediaBrowser.MediaEncoding.Probing;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Configuration;
using System;
using MediaBrowser.Controller.Configuration;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class MediaEncoderTests
    {
        private readonly Mock<ILogger<MediaEncoder>> _mockLogger;
        private readonly Mock<IServerConfigurationManager> _mockConfigurationManager;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IBlurayExaminer> _mockBlurayExaminer;
        private readonly Mock<ILocalizationManager> _mockLocalization;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IServerConfigurationManager> _mockServerConfig;
        private readonly Mock<IEncodingHelper> _mockEncodingHelper;

        public MediaEncoderTests()
        {
            _mockLogger = new Mock<ILogger<MediaEncoder>>();
            _mockConfigurationManager = new Mock<IServerConfigurationManager>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockBlurayExaminer = new Mock<IBlurayExaminer>();
            _mockLocalization = new Mock<ILocalizationManager>();
            _mockConfig = new Mock<IConfiguration>();
            _mockServerConfig = new Mock<IServerConfigurationManager>();
            _mockEncodingHelper = new Mock<IEncodingHelper>();
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarning_WhenIFrameTrickplayExtractionFails()
        {
            // Arrange
            var mediaEncoder = new MediaEncoder(
                _mockLogger.Object,
                _mockConfigurationManager.Object,
                _mockFileSystem.Object,
                _mockBlurayExaminer.Object,
                _mockLocalization.Object,
                _mockConfig.Object,
                _mockServerConfig.Object);

            var jobState = new JobState();
            var options = new EncodingOptions
            {
                HardwareAccelerationType = HardwareAccelerationType.videotoolbox
            };
            var vidEncoder = "libx264";
            var inputFile = "test.mp4";
            var inputArg = "-i test.mp4";
            var filterParam = "filter";
            var threads = 4;
            var qualityScale = 23;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            _mockEncodingHelper.Setup(e => e.GetVideoProcessingFilterParam(jobState, options, vidEncoder))
                .Returns(filterParam);

            mediaEncoder.SetFFmpegPath();

            // Act
            await mediaEncoder.ExtractVideoImagesOnInterval(jobState, options, vidEncoder, inputFile, inputArg, threads, qualityScale, priority, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<FfmpegException>(),
                    It.Is<string>(s => s.Contains("I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
