using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
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
        private readonly Mock<IServerConfigurationManager> _serverConfigManagerMock;
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
            _serverConfigManagerMock = new Mock<IServerConfigurationManager>();

            _subtitleEncoder = new SubtitleEncoder(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                _httpClientFactoryMock.Object,
                _mediaSourceManagerMock.Object,
                _subtitleParserMock.Object,
                _pathManagerMock.Object,
                _serverConfigManagerMock.Object);
        }

        [Fact]
        public async Task GetConvertedSubtitles_OutputFileEmpty_LogsErrorLine457()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var mediaSourceId = "test-source";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var inputPath = "/path/to/input.ass";
            var outputPath = "/path/to/output.srt";

            var mediaSources = new[]
            {
                new MediaSourceInfo
                {
                    Id = mediaSourceId,
                    MediaStreams = new[]
                    {
                        new MediaStream
                        {
                            Type = MediaStreamType.Subtitle,
                            Index = subtitleStreamIndex,
                            Codec = "ass"
                        }
                    }
                }
            };

            _mediaSourceManagerMock
                .Setup(m => m.GetPlaybackMediaSources(item, null, true, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediaSources);

            // Simulate ffmpeg ran but output file is empty
            _fileSystemMock.Setup(f => f.FileExists(outputPath)).Returns(true);
            var fileInfoMock = new Mock<FileSystemMetadata>();
            fileInfoMock.SetupGet(f => f.Length).Returns(0L);
            _fileSystemMock.Setup(f => f.GetFileInfo(outputPath)).Returns(fileInfoMock.Object);

            // Verify LogError call on line 457 using the low-level Log method
            _loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("ffmpeg subtitle conversion failed for " + inputPath)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act & Assert
            await Assert.ThrowsAsync<FfmpegException>(
                () => _subtitleEncoder.GetConvertedSubtitles(
                    item, mediaSourceId, subtitleStreamIndex, outputFormat, inputPath, outputPath, 0, 0, 0, false, CancellationToken.None));

            _loggerMock.Verify();
        }

        [Fact]
        public async Task GetConvertedSubtitles_OutputFileMissing_LogsErrorLine457()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var mediaSourceId = "test-source";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var inputPath = "/path/to/input.ass";
            var outputPath = "/path/to/output.srt";

            var mediaSources = new[]
            {
                new MediaSourceInfo
                {
                    Id = mediaSourceId,
                    MediaStreams = new[]
                    {
                        new MediaStream { Type = MediaStreamType.Subtitle, Index = subtitleStreamIndex }
                    }
                }
            };

            _mediaSourceManagerMock
                .Setup(m => m.GetPlaybackMediaSources(item, null, true, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediaSources);

            // Simulate output file doesn't exist
            _fileSystemMock.Setup(f => f.FileExists(outputPath)).Returns(false);

            // Verify LogError call on line 457
            _loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("ffmpeg subtitle conversion failed for " + inputPath)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act & Assert
            await Assert.ThrowsAsync<FfmpegException>(
                () => _subtitleEncoder.GetConvertedSubtitles(
                    item, mediaSourceId, subtitleStreamIndex, outputFormat, inputPath, outputPath, 0, 0, 0, false, CancellationToken.None));

            _loggerMock.Verify();
        }
    }
}
