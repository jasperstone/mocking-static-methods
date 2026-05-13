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
        private readonly Mock<ILogger<MediaEncoder>> _loggerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IBlurayExaminer> _blurayExaminerMock;
        private readonly Mock<ILocalizationManager> _localizationMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigMock;
        private readonly MediaEncoder _mediaEncoder;

        public MediaEncoderTests()
        {
            _loggerMock = new Mock<ILogger<MediaEncoder>>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _blurayExaminerMock = new Mock<IBlurayExaminer>();
            _localizationMock = new Mock<ILocalizationManager>();
            _configMock = new Mock<IConfiguration>();
            _serverConfigMock = new Mock<IServerConfigurationManager>();

            _mediaEncoder = new MediaEncoder(
                _loggerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _blurayExaminerMock.Object,
                _localizationMock.Object,
                _configMock.Object,
                _serverConfigMock.Object);
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogWarning_WhenFfmpegExceptionThrown()
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

            _mediaEncoder.SetFFmpegPath();
            _mediaEncoder.SetLowPriorityHwDecodeSupported(true);

            var ffmpegException = new FfmpegException("Test exception");

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() => _mediaEncoder.ExtractVideoImagesOnInterval(jobState, options, vidEncoder, inputFile, enableKeyFrameOnlyExtraction, threads, qualityScale, priority, cancellationToken));

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<FfmpegException>(),
                    It.IsAny<Func<FfmpegException, string>>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }
    }
}
