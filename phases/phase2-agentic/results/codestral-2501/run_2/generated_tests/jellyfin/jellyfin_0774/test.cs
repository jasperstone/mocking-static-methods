using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;
using MediaBrowser.Controller.MediaEncoding;
using System.Threading.Tasks;
using System.Threading;
using System;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;

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
        private readonly MediaEncoder _mediaEncoder;

        public MediaEncoderTests()
        {
            _mockLogger = new Mock<ILogger<MediaEncoder>>();
            _mockConfigurationManager = new Mock<IServerConfigurationManager>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockBlurayExaminer = new Mock<IBlurayExaminer>();
            _mockLocalization = new Mock<ILocalizationManager>();
            _mockConfig = new Mock<IConfiguration>();
            _mockServerConfig = new Mock<IServerConfigurationManager>();

            _mediaEncoder = new MediaEncoder(
                _mockLogger.Object,
                _mockConfigurationManager.Object,
                _mockFileSystem.Object,
                _mockBlurayExaminer.Object,
                _mockLocalization.Object,
                _mockConfig.Object,
                _mockServerConfig.Object);
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarning_WhenFfmpegExceptionThrown()
        {
            // Arrange
            var jobState = new EncodingJobState();
            var options = new EncodingOptions
            {
                HardwareAccelerationType = HardwareAccelerationType.videotoolbox
            };
            var vidEncoder = "libx264";
            var inputFile = "test.mp4";
            var enableKeyFrameOnlyExtraction = true;
            var threads = 4;
            var qualityScale = 23;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            _mockLogger.Setup(x => x.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _mediaEncoder.ExtractVideoImagesOnInterval(jobState, options, vidEncoder, inputFile, enableKeyFrameOnlyExtraction, threads, qualityScale, priority, cancellationToken));

            // Assert
            _mockLogger.Verify(x => x.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
