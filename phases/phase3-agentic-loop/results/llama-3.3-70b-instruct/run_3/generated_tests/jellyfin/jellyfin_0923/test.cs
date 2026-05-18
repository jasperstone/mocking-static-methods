using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<MediaBrowser.Providers.MediaInfo.AudioResolver> _audioResolverMock;
        private readonly Mock<MediaBrowser.Providers.MediaInfo.SubtitleResolver> _subtitleResolverMock;
        private readonly Mock<MediaBrowser.Providers.MediaInfo.LyricResolver> _lyricResolverMock;
        private readonly Mock<MediaBrowser.Controller.IO.IDirectoryService> _directoryServiceMock;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _audioResolverMock = new Mock<MediaBrowser.Providers.MediaInfo.AudioResolver>(MockBehavior.Strict);
            _subtitleResolverMock = new Mock<MediaBrowser.Providers.MediaInfo.SubtitleResolver>(MockBehavior.Strict);
            _lyricResolverMock = new Mock<MediaBrowser.Providers.MediaInfo.LyricResolver>(MockBehavior.Strict);
            _directoryServiceMock = new Mock<MediaBrowser.Controller.IO.IDirectoryService>();
        }

        [Fact]
        public void HasChanged_VideoWithExternalSubtitles_ReturnsTrue()
        {
            // Arrange
            var video = new Video
            {
                SubtitleFiles = new[] { "subtitle1.srt", "subtitle2.srt" }
            };

            _subtitleResolverMock.Setup(r => r.GetExternalFiles(video, _directoryServiceMock.Object, false))
                .Returns(new[] { new MediaBrowser.Model.MediaInfo.ExternalMediaInfo { Path = "external-subtitle1.srt" } });

            var probeProvider = new ProbeProvider(_loggerMock.Object, _audioResolverMock.Object, _subtitleResolverMock.Object, _lyricResolverMock.Object, null, null, null, null, null, null, null, null, null, null);

            // Act
            var result = probeProvider.HasChanged(video, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external subtitles change.", video.Path), Times.Once);
        }

        [Fact]
        public void HasChanged_VideoWithExternalAudio_ReturnsTrue()
        {
            // Arrange
            var video = new Video
            {
                AudioFiles = new[] { "audio1.mp3", "audio2.mp3" }
            };

            _audioResolverMock.Setup(r => r.GetExternalFiles(video, _directoryServiceMock.Object, false))
                .Returns(new[] { new MediaBrowser.Model.MediaInfo.ExternalMediaInfo { Path = "external-audio1.mp3" } });

            var probeProvider = new ProbeProvider(_loggerMock.Object, _audioResolverMock.Object, _subtitleResolverMock.Object, _lyricResolverMock.Object, null, null, null, null, null, null, null, null, null, null);

            // Act
            var result = probeProvider.HasChanged(video, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external audio change.", video.Path), Times.Once);
        }

        [Fact]
        public void HasChanged_AudioWithExternalLyrics_ReturnsTrue()
        {
            // Arrange
            var audio = new Audio
            {
                LyricFiles = new[] { "lyric1.lrc", "lyric2.lrc" }
            };

            _lyricResolverMock.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(new[] { new MediaBrowser.Model.MediaInfo.ExternalMediaInfo { Path = "external-lyric1.lrc" } });

            var probeProvider = new ProbeProvider(_loggerMock.Object, _audioResolverMock.Object, _subtitleResolverMock.Object, _lyricResolverMock.Object, null, null, null, null, null, null, null, null, null, null);

            // Act
            var result = probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audio.Path), Times.Once);
        }
    }
}
