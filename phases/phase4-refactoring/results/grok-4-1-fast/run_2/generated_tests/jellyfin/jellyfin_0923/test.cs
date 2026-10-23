#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Naming.Common;
using Emby.Naming.ExternalFiles;

namespace MediaBrowser.Providers.Tests.MediaInfo
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<LyricResolver> _lyricResolverMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _directoryServiceMock = new Mock<IDirectoryService>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<ProbeProvider>()).Returns(_loggerMock.Object);

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
                loggerFactoryMock.Object,
                new Emby.Naming.Common.NamingOptions(),
                Mock.Of<ILyricManager>(),
                Mock.Of<IMediaAttachmentRepository>(),
                Mock.Of<IMediaStreamRepository>());

            // Create and inject mock lyric resolver
            var lyricLogger = new Mock<ILogger<LyricResolver>>();
            _lyricResolverMock = new Mock<LyricResolver>(
                lyricLogger.Object,
                Mock.Of<ILocalizationManager>(),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IFileSystem>(),
                new Emby.Naming.Common.NamingOptions());

            typeof(ProbeProvider).GetField("_lyricResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_probeProvider, _lyricResolverMock.Object);
        }

        [Fact]
        public void HasChanged_AudioWithLyricMismatch_LogsDebugAndReturnsTrue()
        {
            // Arrange
            var audio = new Audio { Path = "/music/song.mp3" };
            audio.SetSupportsLocalMetadata(true);
            audio.LyricFiles = new[] { "/music/song.lrc" };

            var externalFiles = new[] { new ExternalPathParserResult { Path = "/music/song_new.lrc" } };
            _lyricResolverMock.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        ((IEnumerable<KeyValuePair<string, object?>>?)state)?.Any(kvp => 
                            kvp.Key == "ItemPath" && kvp.Value?.ToString() == "/music/song.mp3") == true &&
                        state.ToString()!.Contains("due to external lyrics change")),
                    It.IsAny<Exception?>()),
                Times.Once);
        }

        [Fact]
        public void HasChanged_AudioWithMatchingLyrics_ReturnsFalseNoLog()
        {
            // Arrange
            var audio = new Audio { Path = "/music/song.mp3" };
            audio.SetSupportsLocalMetadata(true);
            audio.LyricFiles = new[] { "/music/song.lrc" };

            var externalFiles = new[] { new ExternalPathParserResult { Path = "/music/song.lrc" } };
            _lyricResolverMock.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("external lyrics change")),
                    It.IsAny<Exception?>()),
                Times.Never);
        }

        [Fact]
        public void HasChanged_AudioWithoutLocalMetadata_DoesNotLogLyricsMessage()
        {
            // Arrange
            var audio = new Audio { Path = "/music/song.mp3" };
            audio.SetSupportsLocalMetadata(false);

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("external lyrics change")),
                    It.IsAny<Exception?>()),
                Times.Never);
        }
    }
}
