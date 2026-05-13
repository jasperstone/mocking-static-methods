using MediaBrowser.Common;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests
{
    public class SubtitleEncoderTests
    {
        private readonly Mock<ILogger<SubtitleEncoder>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IMediaSourceManager> _mediaSourceManagerMock;
        private readonly Mock<ISubtitleParser> _subtitleParserMock;
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;

        public SubtitleEncoderTests()
        {
            _loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            _subtitleParserMock = new Mock<ISubtitleParser>();
            _pathManagerMock = new Mock<IPathManager>();
            _serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public async Task LogErrorCalledWhenFfmpegSubtitleConversionFails()
        {
            // Arrange
            var inputPath = "inputPath";
            var outputPath = "outputPath";
            var failed = true;

            _fileSystemMock.Setup(f => f.DeleteFile(outputPath)).Throws(new IOException());

            var subtitleEncoder = new SubtitleEncoder(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                _httpClientFactoryMock.Object,
                _mediaSourceManagerMock.Object,
                _subtitleParserMock.Object,
                _pathManagerMock.Object,
                _serverConfigurationManagerMock.Object);

            // Act
            try
            {
                await subtitleEncoder.GetSubtitles(
                    new BaseItem(),
                    "mediaSourceId",
                    0,
                    "outputFormat",
                    0,
                    0,
                    false,
                    CancellationToken.None);
            }
            catch (FfmpegException)
            {
            }

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "ffmpeg subtitle conversion failed for {Path}", inputPath), Times.Once);
        }
    }
}
