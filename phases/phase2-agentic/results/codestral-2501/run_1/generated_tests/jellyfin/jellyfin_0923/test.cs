using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<AudioResolver> _audioResolverMock;
        private readonly Mock<SubtitleResolver> _subtitleResolverMock;
        private readonly Mock<LyricResolver> _lyricResolverMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _audioResolverMock = new Mock<AudioResolver>();
            _subtitleResolverMock = new Mock<SubtitleResolver>();
            _lyricResolverMock = new Mock<LyricResolver>();
            _directoryServiceMock = new Mock<IDirectoryService>();

            _probeProvider = new ProbeProvider(
                _loggerMock.Object,
                _audioResolverMock.Object,
                _subtitleResolverMock.Object,
                _lyricResolverMock.Object,
                _directoryServiceMock.Object);
        }

        [Fact]
        public void HasChanged_ShouldLogDebug_WhenExternalSubtitlesChange()
        {
            // Arrange
            var video = new Video
            {
                SupportsLocalMetadata = true,
                IsPlaceHolder = false,
                SubtitleFiles = new List<string> { "subtitle1.srt" }
            };
            var externalFiles = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { Path = "subtitle2.srt" }
            };

            _subtitleResolverMock.Setup(r => r.GetExternalFiles(video, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(video, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void HasChanged_ShouldLogDebug_WhenExternalAudioChange()
        {
            // Arrange
            var video = new Video
            {
                SupportsLocalMetadata = true,
                IsPlaceHolder = false,
                AudioFiles = new List<string> { "audio1.mp3" }
            };
            var externalFiles = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { Path = "audio2.mp3" }
            };

            _audioResolverMock.Setup(r => r.GetExternalFiles(video, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(video, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void HasChanged_ShouldLogDebug_WhenExternalLyricsChange()
        {
            // Arrange
            var audio = new Audio
            {
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyrics1.lrc" }
            };
            var externalFiles = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { Path = "lyrics2.lrc" }
            };

            _lyricResolverMock.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.True(result);
        }
    }
}
