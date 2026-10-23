#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<LyricResolver> _lyricResolverMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerMock = new();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(_loggerMock.Object);
            
            _lyricResolverMock = new();
            _directoryServiceMock = new();

            // Use NullLoggerFactory for ILyricManager parameter
            var nullLogger = NullLoggerFactory.Instance.CreateLogger<ILyricManager>();

            // Create ProbeProvider with minimal viable mocks
            _probeProvider = new ProbeProvider(
                NullObject.Of<IMediaSourceManager>(),
                NullObject.Of<IMediaEncoder>(),
                NullObject.Of<IBlurayExaminer>(),
                NullObject.Of<ILocalizationManager>(),
                NullObject.Of<IChapterManager>(),
                NullObject.Of<IServerConfigurationManager>(),
                NullObject.Of<ISubtitleManager>(),
                NullObject.Of<ILibraryManager>(),
                NullObject.Of<IFileSystem>(),
                loggerFactoryMock.Object,
                new MediaBrowser.Naming.Common.NamingOptions(),
                nullLogger,
                NullObject.Of<IMediaAttachmentRepository>(),
                NullObject.Of<IMediaStreamRepository>()
            );

            // Inject private fields using reflection
            SetPrivateField("_logger", _loggerMock.Object);
            SetPrivateField("_lyricResolver", _lyricResolverMock.Object);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            FieldInfo field = typeof(ProbeProvider)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!;
            field.SetValue(_probeProvider, value);
        }

        [Fact]
        public void HasChanged_AudioWithLyricsMismatch_LogsDebugAndReturnsTrue()
        {
            // Arrange
            var audio = new Audio
            {
                Path = "/music/song.mp3",
                SupportsLocalMetadata = true,
                LyricFiles = ["/music/song.lrc"]
            };

            var externalFiles = [new FileInfo { Path = "/music/song_new.lrc" }];
            _lyricResolverMock
                .Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audio.Path),
                Times.Once);
        }

        [Fact]
        public void HasChanged_AudioWithMatchingLyrics_ReturnsFalseNoLog()
        {
            // Arrange
            var audio = new Audio
            {
                Path = "/music/song.mp3",
                SupportsLocalMetadata = true,
                LyricFiles = ["/music/song.lrc"]
            };

            var externalFiles = [new FileInfo { Path = "/music/song.lrc" }];
            _lyricResolverMock
                .Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogDebug("Refreshing {ItemPath} due to external lyrics change.", It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void HasChanged_NonAudioItem_DoesNotLogLyricsMessage()
        {
            // Arrange
            var nonAudio = new Mock<BaseItem>().Object;
            nonAudio.Path = "/video.mp4";
            nonAudio.SupportsLocalMetadata = true;

            // Act
            _probeProvider.HasChanged(nonAudio, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Refreshing {ItemPath} due to external lyrics change.", It.IsAny<string>()),
                Times.Never);
        }
    }
}
