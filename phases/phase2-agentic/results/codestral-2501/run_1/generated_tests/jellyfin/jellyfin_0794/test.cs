using Xunit;
using Moq;
using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Controller.MediaEncoding;
using System.Net.Http;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    public class SubtitleEncoderTests
    {
        private readonly Mock<ILogger<SubtitleEncoder>> _mockLogger;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IMediaEncoder> _mockMediaEncoder;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IMediaSourceManager> _mockMediaSourceManager;
        private readonly Mock<ISubtitleParser> _mockSubtitleParser;
        private readonly Mock<IPathManager> _mockPathManager;
        private readonly Mock<IServerConfigurationManager> _mockServerConfigurationManager;
        private readonly SubtitleEncoder _subtitleEncoder;

        public SubtitleEncoderTests()
        {
            _mockLogger = new Mock<ILogger<SubtitleEncoder>>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockMediaEncoder = new Mock<IMediaEncoder>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockMediaSourceManager = new Mock<IMediaSourceManager>();
            _mockSubtitleParser = new Mock<ISubtitleParser>();
            _mockPathManager = new Mock<IPathManager>();
            _mockServerConfigurationManager = new Mock<IServerConfigurationManager>();

            _subtitleEncoder = new SubtitleEncoder(
                _mockLogger.Object,
                _mockFileSystem.Object,
                _mockMediaEncoder.Object,
                _mockHttpClientFactory.Object,
                _mockMediaSourceManager.Object,
                _mockSubtitleParser.Object,
                _mockPathManager.Object,
                _mockServerConfigurationManager.Object);
        }

        [Fact]
        public async Task GetSubtitles_ShouldLogError_WhenDeletingConvertedSubtitleFails()
        {
            // Arrange
            var item = new BaseItem();
            var mediaSourceId = "mediaSourceId";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 0L;
            var preserveOriginalTimestamps = false;
            var cancellationToken = CancellationToken.None;
            var outputPath = "outputPath";

            _mockMediaSourceManager.Setup(m => m.GetPlaybackMediaSources(item, null, true, false, cancellationToken))
                .ReturnsAsync(new List<MediaSourceInfo>
                {
                    new MediaSourceInfo
                    {
                        Id = mediaSourceId,
                        MediaStreams = new List<MediaStream>
                        {
                            new MediaStream { Type = MediaStreamType.Subtitle, Index = subtitleStreamIndex }
                        }
                    }
                });

            _mockFileSystem.Setup(f => f.GetFileInfo(outputPath)).Returns(new FileSystemMetadata { Length = 0 });
            _mockFileSystem.Setup(f => f.DeleteFile(outputPath)).Throws(new IOException());

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() => _subtitleEncoder.GetSubtitles(item, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken));

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<IOException>(),
                    "Error deleting converted subtitle {Path}",
                    outputPath),
                Times.Once);
        }

        [Fact]
        public async Task GetSubtitles_ShouldLogError_WhenFfmpegSubtitleConversionFails()
        {
            // Arrange
            var item = new BaseItem();
            var mediaSourceId = "mediaSourceId";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 0L;
            var preserveOriginalTimestamps = false;
            var cancellationToken = CancellationToken.None;
            var inputPath = "inputPath";

            _mockMediaSourceManager.Setup(m => m.GetPlaybackMediaSources(item, null, true, false, cancellationToken))
                .ReturnsAsync(new List<MediaSourceInfo>
                {
                    new MediaSourceInfo
                    {
                        Id = mediaSourceId,
                        MediaStreams = new List<MediaStream>
                        {
                            new MediaStream { Type = MediaStreamType.Subtitle, Index = subtitleStreamIndex }
                        }
                    }
                });

            _mockFileSystem.Setup(f => f.GetFileInfo(inputPath)).Returns(new FileSystemMetadata { Length = 0 });
            _mockFileSystem.Setup(f => f.DeleteFile(inputPath)).Throws(new IOException());

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() => _subtitleEncoder.GetSubtitles(item, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken));

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    "ffmpeg subtitle conversion failed for {Path}",
                    inputPath),
                Times.Once);
        }
    }
}
