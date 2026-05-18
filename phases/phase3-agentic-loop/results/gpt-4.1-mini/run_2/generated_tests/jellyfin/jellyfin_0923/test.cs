using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.IO;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(_loggerMock.Object);

            // Provide mocks for constructor dependencies
            var mediaSourceManager = Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaSourceManager>();
            var mediaEncoder = Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var blurayExaminer = Mock.Of<MediaBrowser.Controller.Configuration.IBlurayExaminer>();
            var localization = Mock.Of<MediaBrowser.Model.Globalization.ILocalizationManager>();
            var chapterManager = Mock.Of<MediaBrowser.Controller.Chapters.IChapterManager>();
            var config = Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var subtitleManager = Mock.Of<MediaBrowser.Controller.Subtitles.ISubtitleManager>();
            var libraryManager = Mock.Of<MediaBrowser.Controller.Library.ILibraryManager>();
            var fileSystem = Mock.Of<MediaBrowser.Model.IO.IFileSystem>();
            var namingOptions = new Emby.Naming.Common.NamingOptions();
            var lyricManager = Mock.Of<MediaBrowser.Controller.Lyrics.ILyricManager>();
            var mediaAttachmentRepository = Mock.Of<MediaBrowser.Controller.Persistence.IMediaAttachmentRepository>();
            var mediaStreamRepository = Mock.Of<MediaBrowser.Controller.Persistence.IMediaStreamRepository>();

            _probeProvider = new ProbeProvider(
                mediaSourceManager,
                mediaEncoder,
                blurayExaminer,
                localization,
                chapterManager,
                config,
                subtitleManager,
                libraryManager,
                fileSystem,
                _loggerFactoryMock.Object,
                namingOptions,
                lyricManager,
                mediaAttachmentRepository,
                mediaStreamRepository);

            _directoryServiceMock = new Mock<IDirectoryService>();
        }

        [Fact]
        public void HasChanged_LogsDebug_WhenAudioExternalLyricsChanged()
        {
            // Arrange
            var audio = new Audio
            {
                Path = "audioPath",
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1.lrc" }
            };

            // Setup the lyric resolver to return different external files than audio.LyricFiles
            var lyricResolverMock = new Mock<MediaBrowser.Providers.MediaInfo.LyricResolver>(
                Mock.Of<ILogger<MediaBrowser.Providers.MediaInfo.LyricResolver>>(),
                Mock.Of<MediaBrowser.Model.Globalization.ILocalizationManager>(),
                Mock.Of<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>(),
                Mock.Of<MediaBrowser.Model.IO.IFileSystem>(),
                new Emby.Naming.Common.NamingOptions());

            lyricResolverMock.Setup(lr => lr.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(new List<MediaBrowser.Model.IO.FileSystemMetadata>
                {
                    new MediaBrowser.Model.IO.FileSystemMetadata { Path = "different.lyrics" }
                });

            // Replace the private _lyricResolver field with our mock
            var lyricResolverField = typeof(ProbeProvider).GetField("_lyricResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            lyricResolverField.SetValue(_probeProvider, lyricResolverMock.Object);

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing audioPath due to external lyrics change.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
