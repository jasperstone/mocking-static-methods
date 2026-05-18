using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Tests.MediaInfo
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(_loggerMock.Object);

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
        }

        [Fact]
        public void HasChanged_AudioItem_LyricsMismatch_LogsDebugMessage()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "/music/song.mp3",
                LyricFiles = new List<string> { "/music/song.lrc" }.AsReadOnly(),
                SupportsLocalMetadata = true
            };

            // Mock the private _lyricResolver field
            var lyricResolverMock = new Mock<MediaBrowser.Controller.Lyrics.LyricResolver>(
                Mock.Of<ILogger<MediaBrowser.Controller.Lyrics.LyricResolver>>(),
                Mock.Of<ILocalizationManager>(),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IFileSystem>(),
                new Emby.Naming.Common.NamingOptions());

            var externalFiles = new List<MediaBrowser.Model.IO.FileInfo> 
            { 
                new() { Path = "/music/song_new.lrc" } 
            };
            
            lyricResolverMock.Setup(r => r.GetExternalFiles(audioItem, It.IsAny<IDirectoryService>(), false))
                           .Returns(externalFiles);

            var field = typeof(ProbeProvider).GetField("_lyricResolver", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_probeProvider, lyricResolverMock.Object);

            // Mock directory service
            var directoryServiceMock = new Mock<IDirectoryService>();

            // Act
            var result = _probeProvider.HasChanged(audioItem, directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Refreshing /music/song.mp3 due to external lyrics change.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void HasChanged_AudioItem_LyricsMatch_NoLogMessage()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "/music/song.mp3",
                LyricFiles = new List<string> { "/music/song.lrc" }.AsReadOnly(),
                SupportsLocalMetadata = true
            };

            var lyricResolverMock = new Mock<MediaBrowser.Controller.Lyrics.LyricResolver>(
                Mock.Of<ILogger<MediaBrowser.Controller.Lyrics.LyricResolver>>(),
                Mock.Of<ILocalizationManager>(),
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IFileSystem>(),
                new Emby.Naming.Common.NamingOptions());
            
            var externalFiles = new List<MediaBrowser.Model.IO.FileInfo> 
            { 
                new() { Path = "/music/song.lrc" } 
            };
            
            lyricResolverMock.Setup(r => r.GetExternalFiles(audioItem, It.IsAny<IDirectoryService>(), false))
                           .Returns(externalFiles);

            var field = typeof(ProbeProvider).GetField("_lyricResolver", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_probeProvider, lyricResolverMock.Object);

            var directoryServiceMock = new Mock<IDirectoryService>();

            // Act
            var result = _probeProvider.HasChanged(audioItem, directoryServiceMock.Object);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
