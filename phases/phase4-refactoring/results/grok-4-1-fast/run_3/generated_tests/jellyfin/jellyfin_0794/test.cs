using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Subtitles;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Subtitles
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
        public void GetExtractableSubtitleFormat_AssCodec_ReturnsAss()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "ass" };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("ass", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_SsaCodec_ReturnsSsa()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "ssa" };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("ssa", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_PgsSubCodec_ReturnsPgsSub()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "pgssub" };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("pgssub", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_OtherCodec_ReturnsSrt()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "vobsub" };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("srt", result);
        }

        [Fact]
        public void GetExtractableSubtitleFileExtension_PgsSubCodec_ReturnsSup()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "pgssub" };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFileExtension(subtitleStream);

            // Assert
            Assert.Equal("sup", result);
        }

        [Fact]
        public void GetExtractableSubtitleFileExtension_OtherCodec_ReturnsCodec()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "ass" };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFileExtension(subtitleStream);

            // Assert
            Assert.Equal("ass", result);
        }

        [Fact]
        public async Task SubtitleConversion_Fails_LogsErrorMessage_Line457()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var mediaSourceId = "test-source";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var inputPath = "/path/to/input.srt";
            var outputPath = "/path/to/output.srt";

            var mediaSource = new MediaSourceInfo { Id = mediaSourceId };
            var subtitleStream = new MediaStream { Index = subtitleStreamIndex, Type = MediaStreamType.Subtitle };
            mediaSource.MediaStreams.Add(subtitleStream);

            _mediaSourceManagerMock
                .Setup(m => m.GetPlaybackMediaSources(item, null, true, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { mediaSource });

            _fileSystemMock.Setup(f => f.GetReadableFile(mediaSource, subtitleStream, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SubtitleInfo { Path = inputPath, Format = "srt" });

            _mediaEncoderMock.Setup(m => m.EncodeVideo(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<VideoProcessingArguments>(),
                It.IsAny<EncodingJobInfo>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(outputPath);

            _fileSystemMock.Setup(f => f.GetFileInfo(outputPath))
                .Returns(new FileSystemMetadata { Length = 0 });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MediaBrowser.Controller.MediaEncoding.FfmpegException>(
                () => ((ISubtitleEncoder)_subtitleEncoder).GetSubtitles(item, mediaSourceId, subtitleStreamIndex, outputFormat, 0, 0, false, CancellationToken.None));

            // Verify the specific LogError call on line 457: _logger.LogError("ffmpeg subtitle conversion failed for {Path}", inputPath);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    0,
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("ffmpeg subtitle conversion failed for " + inputPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
