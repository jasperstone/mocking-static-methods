using System;
using System.Collections.Generic;
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
        public void GetExtractableSubtitleFormat_AssCodec_ReturnsAss()
        {
            // Arrange
            var subtitleStream = new MediaStream
            {
                Codec = "ass"
            };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("ass", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_SsaCodec_ReturnsSsa()
        {
            // Arrange
            var subtitleStream = new MediaStream
            {
                Codec = "ssa"
            };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("ssa", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_PgsSubCodec_ReturnsPgsSub()
        {
            // Arrange
            var subtitleStream = new MediaStream
            {
                Codec = "pgssub"
            };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("pgssub", result);
        }

        [Fact]
        public void GetExtractableSubtitleFormat_OtherCodec_ReturnsSrt()
        {
            // Arrange
            var subtitleStream = new MediaStream
            {
                Codec = "vobsub"
            };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFormat(subtitleStream);

            // Assert
            Assert.Equal("srt", result);
        }

        [Fact]
        public void GetExtractableSubtitleFileExtension_PgsSubCodec_ReturnsSup()
        {
            // Arrange
            var subtitleStream = new MediaStream
            {
                Codec = "pgssub"
            };

            // Act
            var result = _subtitleEncoder.GetExtractableSubtitleFileExtension(subtitleStream);

            // Assert
            Assert.Equal("sup", result);
        }
    }
}
