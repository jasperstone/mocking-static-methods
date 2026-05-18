using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Audio;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<ILyricResolver> _lyricResolverMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _directoryServiceMock = new Mock<IDirectoryService>();
            _lyricResolverMock = new Mock<ILyricResolver>();

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
                Mock.Of<ILoggerFactory>(),
                new NamingOptions(),
                Mock.Of<ILyricManager>(),
                Mock.Of<IMediaAttachmentRepository>(),
                Mock.Of<IMediaStreamRepository>())
            {
                _logger = _loggerMock.Object,
                _lyricResolver = _lyricResolverMock.Object
            };
        }

        [Fact]
        public void HasChanged_ShouldLogDebug_WhenExternalLyricsChange()
        {
            // Arrange
            var audio = new Audio
            {
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1.txt" }
            };

            _lyricResolverMock.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(new List<FileSystemMetadata> { new FileSystemMetadata { Path = "lyric2.txt" } });

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing {ItemPath} due to external lyrics change.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void HasChanged_ShouldNotLogDebug_WhenNoExternalLyricsChange()
        {
            // Arrange
            var audio = new Audio
            {
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1.txt" }
            };

            _lyricResolverMock.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(new List<FileSystemMetadata> { new FileSystemMetadata { Path = "lyric1.txt" } });

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing {ItemPath} due to external lyrics change.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
            Assert.False(result);
        }
    }
}
