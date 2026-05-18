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
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.MediaEncoding.Probing;
using Microsoft.Extensions.Configuration;
using System;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
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
        private readonly Mock<IEncodingHelper> _encodingHelperMock;

        public MediaEncoderTests()
        {
            _loggerMock = new Mock<ILogger<MediaEncoder>>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _blurayExaminerMock = new Mock<IBlurayExaminer>();
            _localizationMock = new Mock<ILocalizationManager>();
            _configMock = new Mock<IConfiguration>();
            _serverConfigMock = new Mock<IServerConfigurationManager>();
            _encodingHelperMock = new Mock<IEncodingHelper>();
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogWarning_WhenIFrameTrickplayExtractionFails()
        {
            // Arrange
            var mediaEncoder = new MediaEncoder(
                _loggerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _blurayExaminerMock.Object,
                _localizationMock.Object,
                _configMock.Object,
                _serverConfigMock.Object);

            var jobState = new EncodingJobState();
            var options = new EncodingOptions
            {
                HardwareAccelerationType = HardwareAccelerationType.videotoolbox
            };
            var vidEncoder = "someEncoder";
            var inputFile = "inputFile";
            var inputArg = "inputArg";
            var filterParam = "filterParam";
            var threads = 1;
            var qualityScale = 1;
            var priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            _encodingHelperMock.Setup(e => e.GetVideoProcessingFilterParam(jobState, options, vidEncoder))
                .Returns(filterParam);

            mediaEncoder.SetEncodingHelper(_encodingHelperMock.Object);

            // Act
            await mediaEncoder.ExtractVideoImagesOnInterval(jobState, options, vidEncoder, inputFile, inputArg, threads, qualityScale, priority, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<FfmpegException>(),
                    It.IsAny<Func<FfmpegException, Exception, string>>(),
                    It.IsAny<FfmpegException>(),
                    It.IsAny<Func<FfmpegException, Exception, string>>()),
                Times.Once);
        }
    }
}
