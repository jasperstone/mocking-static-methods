using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UtfUnknown;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
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
        public async Task LogError_Called_When_Ffmpeg_Subtitle_Conversion_Fails()
        {
            // Arrange
            var subtitleEncoder = new SubtitleEncoder(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                _httpClientFactoryMock.Object,
                _mediaSourceManagerMock.Object,
                _subtitleParserMock.Object,
                _pathManagerMock.Object,
                _serverConfigurationManagerMock.Object);

            var inputPath = "inputPath";
            var outputPath = "outputPath";

            _fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws(new IOException());

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => ((ISubtitleEncoder)subtitleEncoder).GetSubtitles(null, null, 0, null, 0, 0, false, CancellationToken.None));

            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "ffmpeg subtitle conversion failed for {Path}", inputPath), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_Deleting_Converted_Subtitle_Fails()
        {
            // Arrange
            var subtitleEncoder = new SubtitleEncoder(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                _httpClientFactoryMock.Object,
                _mediaSourceManagerMock.Object,
                _subtitleParserMock.Object,
                _pathManagerMock.Object,
                _serverConfigurationManagerMock.Object);

            var inputPath = "inputPath";
            var outputPath = "outputPath";

            _fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws(new IOException());

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => ((ISubtitleEncoder)subtitleEncoder).GetSubtitles(null, null, 0, null, 0, 0, false, CancellationToken.None));

            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error deleting converted subtitle {Path}", outputPath), Times.Once);
        }
    }
}
