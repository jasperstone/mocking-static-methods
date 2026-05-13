using Xunit;
using Moq;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Metadata;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Providers.MediaInfo
{
    public class ProbeProviderTests
    {
        [Fact]
        public async Task HasChanged_LogsDebugMessage_WhenExternalLyricsChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var audioMock = new Mock<Audio>();
            var lyricResolverMock = new Mock<ILyricResolver>();

            var probeProvider = new ProbeProvider(
                loggerMock.Object,
                new AudioResolver(new Mock<ILogger<AudioResolver>>().Object, new Mock<ILocalizationManager>().Object, new Mock<IMediaEncoder>().Object, new Mock<IFileSystem>().Object, new Mock<NamingOptions>().Object),
                new SubtitleResolver(new Mock<ILogger<SubtitleResolver>>().Object, new Mock<ILocalizationManager>().Object, new Mock<IMediaEncoder>().Object, new Mock<IFileSystem>().Object, new Mock<NamingOptions>().Object),
                lyricResolverMock.Object,
                new Mock<FFProbeVideoInfo>(),
                new Mock<AudioFileProber>());

            audioMock.SetupGet(a => a.LyricFiles).Returns(new List<string> { "lyric1" });
            lyricResolverMock.Setup(lr => lr.GetExternalFiles(audioMock.Object, directoryServiceMock.Object, false)).Returns(new List<ExternalMediaInfo> { new ExternalMediaInfo { Path = "lyric2" } });

            // Act
            var result = probeProvider.HasChanged(audioMock.Object, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audioMock.Object.Path), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task HasChanged_LogsDebugMessage_WhenExternalSubtitlesChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var videoMock = new Mock<Video>();
            var subtitleResolverMock = new Mock<ISubtitleResolver>();

            var probeProvider = new ProbeProvider(
                loggerMock.Object,
                new AudioResolver(new Mock<ILogger<AudioResolver>>().Object, new Mock<ILocalizationManager>().Object, new Mock<IMediaEncoder>().Object, new Mock<IFileSystem>().Object, new Mock<NamingOptions>().Object),
                subtitleResolverMock.Object,
                new Mock<ILyricResolver>(),
                new Mock<FFProbeVideoInfo>(),
                new Mock<AudioFileProber>());

            videoMock.SetupGet(v => v.SubtitleFiles).Returns(new List<string> { "subtitle1" });
            subtitleResolverMock.Setup(sr => sr.GetExternalFiles(videoMock.Object, directoryServiceMock.Object, false)).Returns(new List<ExternalMediaInfo> { new ExternalMediaInfo { Path = "subtitle2" } });

            // Act
            var result = probeProvider.HasChanged(videoMock.Object, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external subtitles change.", videoMock.Object.Path), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task HasChanged_LogsDebugMessage_WhenExternalAudioChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var videoMock = new Mock<Video>();
            var audioResolverMock = new Mock<IAudioResolver>();

            var probeProvider = new ProbeProvider(
                loggerMock.Object,
                audioResolverMock.Object,
                new Mock<ISubtitleResolver>(),
                new Mock<ILyricResolver>(),
                new Mock<FFProbeVideoInfo>(),
                new Mock<AudioFileProber>());

            videoMock.SetupGet(v => v.AudioFiles).Returns(new List<string> { "audio1" });
            audioResolverMock.Setup(ar => ar.GetExternalFiles(videoMock.Object, directoryServiceMock.Object, false)).Returns(new List<ExternalMediaInfo> { new ExternalMediaInfo { Path = "audio2" } });

            // Act
            var result = probeProvider.HasChanged(videoMock.Object, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external audio change.", videoMock.Object.Path), Times.Once);
            Assert.True(result);
        }
    }
}
