using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<LyricResolver> _lyricResolverMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(_loggerMock.Object);
            _lyricResolverMock = new Mock<LyricResolver>();
            _directoryServiceMock = new Mock<IDirectoryService>();

            _probeProvider = new ProbeProvider(
                Mock.Of<IMediaSourceManager>(),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IBlurayExaminer>(),
                Mock.Of<ILocalizationManager>(),
                Mock.Of<IChapterManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<ISubtitleManager>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IFileSystem>(),
                _loggerFactoryMock.Object,
                new Emby.Naming.Common.NamingOptions(),
                Mock.Of<ILyricManager>(),
                Mock.Of<IMediaAttachmentRepository>(),
                Mock.Of<IMediaStreamRepository>());

            // Inject mocks via reflection
            typeof(ProbeProvider).GetField("_lyricResolver", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_probeProvider, _lyricResolverMock.Object);
        }

        [Fact]
        public void HasChanged_AudioWithLyricMismatch_LogsDebugMessage()
        {
            // Arrange
            var audio = new Audio { Path = "/music/song.mp3" };
            audio.SetSupportsLocalMetadata(true); // Use setter if available or reflection

            var externalFiles = new List<Emby.Naming.ExternalFiles.ExternalPathParserResult>
            {
                new() { Path = "/music/song_new.lrc" }
            };
            _lyricResolverMock.Setup(x => x.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            audio.LyricFiles = new[] { "/music/song.lrc" };

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Refreshing /music/song.mp3 due to external lyrics change.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void HasChanged_AudioWithMatchingLyrics_DoesNotLogLyricsMessage()
        {
            // Arrange
            var audio = new Audio { Path = "/music/song.mp3" };
            audio.SetSupportsLocalMetadata(true);

            var externalFiles = new List<Emby.Naming.ExternalFiles.ExternalPathParserResult>
            {
                new() { Path = "/music/song.lrc" }
            };
            _lyricResolverMock.Setup(x => x.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            audio.LyricFiles = new[] { "/music/song.lrc" };

            // Act
            _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("external lyrics change")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void HasChanged_NonAudioItem_DoesNotLogLyricsMessage()
        {
            // Arrange
            var video = new Video { Path = "/video/movie.mkv" };
            video.SetSupportsLocalMetadata(true);

            // Act
            _probeProvider.HasChanged(video, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("external lyrics change")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
