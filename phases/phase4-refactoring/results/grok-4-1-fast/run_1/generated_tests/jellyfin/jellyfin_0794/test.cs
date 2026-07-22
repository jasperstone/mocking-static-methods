using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
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
        private readonly Mock<IServerConfigurationManager> _serverConfigManagerMock;

        public SubtitleEncoderTests()
        {
            _loggerMock = new();
            _fileSystemMock = new();
            _mediaEncoderMock = new();
            _httpClientFactoryMock = new();
            _mediaSourceManagerMock = new();
            _subtitleParserMock = new();
            _pathManagerMock = new();
            _serverConfigManagerMock = new();
        }

        [Fact]
        public void GetExtractableSubtitleFormat_AssCodec_ReturnsAss()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "ass" };
            var encoder = CreateSubtitleEncoder();

            // Act
            var result = encoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("ass", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_SsaCodec_ReturnsSsa()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "ssa" };
            var encoder = CreateSubtitleEncoder();

            // Act
            var result = encoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("ssa", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_PgsSubCodec_ReturnsPgsSub()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "pgssub" };
            var encoder = CreateSubtitleEncoder();

            // Act
            var result = encoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("pgssub", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_OtherCodec_ReturnsSrt()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "vobsub" };
            var encoder = CreateSubtitleEncoder();

            // Act
            var result = encoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("srt", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_CaseInsensitiveAss_ReturnsAss()
        {
            // Arrange
            var subtitleStream = new MediaStream { Codec = "ASS" };
            var encoder = CreateSubtitleEncoder();

            // Act
            var result = encoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("ass", result);
        }

        private SubtitleEncoder CreateSubtitleEncoder()
        {
            return new SubtitleEncoder(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                _httpClientFactoryMock.Object,
                _mediaSourceManagerMock.Object,
                _subtitleParserMock.Object,
                _pathManagerMock.Object,
                _serverConfigManagerMock.Object);
        }
    }
}
