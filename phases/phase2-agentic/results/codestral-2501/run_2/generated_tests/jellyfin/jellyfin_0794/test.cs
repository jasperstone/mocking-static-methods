using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
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
        private readonly SubtitleEncoder _subtitleEncoder;

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

            _subtitleEncoder = new SubtitleEncoder(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                _httpClientFactoryMock.Object,
                _mediaSourceManagerMock.Object,
                _subtitleParserMock.Object,
                _pathManagerMock.Object,
                _serverConfigurationManagerMock.Object);
        }

        [Fact]
        public async Task ConvertSubtitles_DeletesConvertedSubtitleOnIOException()
        {
            // Arrange
            var inputPath = "inputPath";
            var outputPath = "outputPath";
            var cancellationToken = CancellationToken.None;
            var item = new BaseItem();
            var mediaSourceId = "mediaSourceId";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 0L;
            var preserveOriginalTimestamps = false;

            _fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws(new IOException());

            // Act
            await Assert.ThrowsAsync<IOException>(() => _subtitleEncoder.GetSubtitles(item, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken));

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<IOException>(), "Error deleting converted subtitle {Path}", outputPath), Times.Once);
        }

        [Fact]
        public async Task ConvertSubtitles_DeletesConvertedSubtitleDueToFailureOnIOException()
        {
            // Arrange
            var inputPath = "inputPath";
            var outputPath = "outputPath";
            var cancellationToken = CancellationToken.None;
            var item = new BaseItem();
            var mediaSourceId = "mediaSourceId";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 0L;
            var preserveOriginalTimestamps = false;

            _fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws(new IOException());

            // Act
            await Assert.ThrowsAsync<IOException>(() => _subtitleEncoder.GetSubtitles(item, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken));

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<IOException>(), "Error deleting converted subtitle {Path}", outputPath), Times.Once);
        }

        [Fact]
        public async Task ConvertSubtitles_FailsOnFfmpegException()
        {
            // Arrange
            var inputPath = "inputPath";
            var outputPath = "outputPath";
            var cancellationToken = CancellationToken.None;
            var item = new BaseItem();
            var mediaSourceId = "mediaSourceId";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 0L;
            var preserveOriginalTimestamps = false;

            _fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath)).Returns(new FileInfo(outputPath) { Length = 0 });

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() => _subtitleEncoder.GetSubtitles(item, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken));

            // Assert
            _loggerMock.Verify(logger => logger.LogError("ffmpeg subtitle conversion failed for {Path}", inputPath), Times.Once);
        }
    }
}
