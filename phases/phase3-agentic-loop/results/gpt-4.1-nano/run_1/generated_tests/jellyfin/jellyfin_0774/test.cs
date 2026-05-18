using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using System.Threading;

namespace MediaEncoderTests
{
    public class MediaEncoderTests
    {
        private readonly Mock<ILogger<MediaEncoder>> _loggerMock;
        private readonly Mock<IServerConfigurationManager> _configManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IBlurayExaminer> _blurayExaminerMock;
        private readonly Mock<ILocalizationManager> _localizationMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigMock;

        public MediaEncoderTests()
        {
            _loggerMock = new Mock<ILogger<MediaEncoder>>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _blurayExaminerMock = new Mock<IBlurayExaminer>();
            _localizationMock = new Mock<ILocalizationManager>();
            _configurationMock = new Mock<IConfiguration>();
            _serverConfigMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public void SetFFmpegPath_ShouldLogWarningAndReturnTrue_WhenSkipValidationIsTrue()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetFFmpegSkipValidation()).Returns(true);
            var encoder = new Mock<MediaEncoder>(_loggerMock.Object, _configManagerMock.Object, _fileSystemMock.Object, _blurayExaminerMock.Object, _localizationMock.Object, config.Object, _serverConfigMock.Object);
            encoder.CallBase = true;

            // Act
            var result = encoder.Object.SetFFmpegPath();

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void SetFFmpegPath_ShouldUseConfiguredPath_WhenPathIsValid()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetFFmpegSkipValidation()).Returns(false);
            config.Setup(c => c.GetValue<string>(It.IsAny<string>())).Returns<string>(null);
            var encoder = new Mock<MediaEncoder>(_loggerMock.Object, _configManagerMock.Object, _fileSystemMock.Object, _blurayExaminerMock.Object, _localizationMock.Object, config.Object, _serverConfigMock.Object);
            encoder.CallBase = true;

            encoder.Setup(e => e.ValidatePath(It.IsAny<string>())).Returns(true);
            encoder.SetupGet(e => e._startupOptionFFmpegPath).Returns(string.Empty);
            _configManagerMock.Setup(c => c.GetEncodingOptions()).Returns(new EncodingOptions { EncoderAppPath = "validPath" });

            // Act
            var result = encoder.Object.SetFFmpegPath();

            // Assert
            Assert.True(result);
            Assert.Equal("validPath", encoder.Object.EncoderPath);
        }

        [Fact]
        public void SetFFmpegPath_ShouldDefaultToFfmpeg_WhenNoPathConfiguredAndValidationSucceeds()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetFFmpegSkipValidation()).Returns(false);
            config.Setup(c => c.GetValue<string>(It.IsAny<string>())).Returns<string>(null);
            var encoder = new Mock<MediaEncoder>(_loggerMock.Object, _configManagerMock.Object, _fileSystemMock.Object, _blurayExaminerMock.Object, _localizationMock.Object, config.Object, _serverConfigMock.Object);
            encoder.CallBase = true;

            encoder.Setup(e => e.ValidatePath(It.IsAny<string>())).Returns(true);
            encoder.SetupGet(e => e._startupOptionFFmpegPath).Returns(string.Empty);
            _configManagerMock.Setup(c => c.GetEncodingOptions()).Returns(new EncodingOptions { EncoderAppPath = null });

            // Act
            var result = encoder.Object.SetFFmpegPath();

            // Assert
            Assert.True(result);
            Assert.Equal("ffmpeg", encoder.Object.EncoderPath);
        }

        [Fact]
        public void SetFFmpegPath_ShouldReturnFalseAndLogError_WhenValidationFails()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.GetFFmpegSkipValidation()).Returns(false);
            config.Setup(c => c.GetValue<string>(It.IsAny<string>())).Returns<string>(null);
            var encoder = new Mock<MediaEncoder>(_loggerMock.Object, _configManagerMock.Object, _fileSystemMock.Object, _blurayExaminerMock.Object, _localizationMock, config.Object, _serverConfigMock.Object);
            encoder.CallBase = true;

            encoder.Setup(e => e.ValidatePath(It.IsAny<string>())).Returns(false);
            encoder.SetupGet(e => e._startupOptionFFmpegPath).Returns(string.Empty);
            _configManagerMock.Setup(c => c.GetEncodingOptions()).Returns(new EncodingOptions { EncoderAppPath = null });

            // Act
            var result = encoder.Object.SetFFmpegPath();

            // Assert
            Assert.False(result);
            Assert.Null(encoder.Object.EncoderPath);
        }
    }

    public class EncodingOptions : IEncodingOptions
    {
        public string EncoderAppPath { get; set; }
    }
}
