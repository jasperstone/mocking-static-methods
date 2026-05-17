using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
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
        private readonly Mock<FFProbeVideoInfo> _videoProberMock;
        private readonly Mock<AudioFileProber> _audioProberMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IMediaSourceManager> _mediaSourceManagerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IBlurayExaminer> _blurayExaminerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly Mock<IChapterManager> _chapterManagerMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;
        private readonly Mock<ISubtitleManager> _subtitleManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<NamingOptions> _namingOptionsMock;
        private readonly Mock<ILyricManager> _lyricManagerMock;
        private readonly Mock<IMediaAttachmentRepository> _mediaAttachmentRepositoryMock;
        private readonly Mock<IMediaStreamRepository> _mediaStreamRepositoryMock;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _audioResolverMock = new Mock<AudioResolver>();
            _subtitleResolverMock = new Mock<SubtitleResolver>();
            _lyricResolverMock = new Mock<LyricResolver>();
            _videoProberMock = new Mock<FFProbeVideoInfo>();
            _audioProberMock = new Mock<AudioFileProber>();
            _directoryServiceMock = new Mock<IDirectoryService>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _blurayExaminerMock = new Mock<IBlurayExaminer>();
            _localizationManagerMock = new Mock<ILocalizationManager>();
            _chapterManagerMock = new Mock<IChapterManager>();
            _serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            _subtitleManagerMock = new Mock<ISubtitleManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _namingOptionsMock = new Mock<NamingOptions>();
            _lyricManagerMock = new Mock<ILyricManager>();
            _mediaAttachmentRepositoryMock = new Mock<IMediaAttachmentRepository>();
            _mediaStreamRepositoryMock = new Mock<IMediaStreamRepository>();
        }

        [Fact]
        public void HasChanged_FileProtocol_ReturnsTrue()
        {
            // Arrange
            var item = new Video
            {
                Path = "path"
            };

            var file = new FileSystemMetadata("path", false, 0, DateTime.Now);
            _directoryServiceMock.Setup(ds => ds.GetFile("path")).Returns(file);

            var probeProvider = new ProbeProvider(
                _mediaSourceManagerMock.Object,
                _mediaEncoderMock.Object,
                _blurayExaminerMock.Object,
                _localizationManagerMock.Object,
                _chapterManagerMock.Object,
                _serverConfigurationManagerMock.Object,
                _subtitleManagerMock.Object,
                _libraryManagerMock.Object,
                _fileSystemMock.Object,
                _loggerFactoryMock.Object,
                _namingOptionsMock.Object,
                _lyricManagerMock.Object,
                _mediaAttachmentRepositoryMock.Object,
                _mediaStreamRepositoryMock.Object);

            // Act
            var result = probeProvider.HasChanged(item, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasChanged_SubtitleFilesChanged_ReturnsTrue()
        {
            // Arrange
            var item = new Video
            {
                Path = "path",
                SubtitleFiles = new[] { "subtitle1" }
            };

            _subtitleResolverMock.Setup(sr => sr.GetExternalFiles(item, _directoryServiceMock.Object, false))
                .Returns(new[] { new ExternalFile { Path = "subtitle2" } });

            var probeProvider = new ProbeProvider(
                _mediaSourceManagerMock.Object,
                _mediaEncoderMock.Object,
                _blurayExaminerMock.Object,
                _localizationManagerMock.Object,
                _chapterManagerMock.Object,
                _serverConfigurationManagerMock.Object,
                _subtitleManagerMock.Object,
                _libraryManagerMock.Object,
                _fileSystemMock.Object,
                _loggerFactoryMock.Object,
                _namingOptionsMock.Object,
                _lyricManagerMock.Object,
                _mediaAttachmentRepositoryMock.Object,
                _mediaStreamRepositoryMock.Object);

            // Act
            var result = probeProvider.HasChanged(item, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasChanged_AudioFilesChanged_ReturnsTrue()
        {
            // Arrange
            var item = new Video
            {
                Path = "path",
                AudioFiles = new[] { "audio1" }
            };

            _audioResolverMock.Setup(ar => ar.GetExternalFiles(item, _directoryServiceMock.Object, false))
                .Returns(new[] { new ExternalFile { Path = "audio2" } });

            var probeProvider = new ProbeProvider(
                _mediaSourceManagerMock.Object,
                _mediaEncoderMock.Object,
                _blurayExaminerMock.Object,
                _localizationManagerMock.Object,
                _chapterManagerMock.Object,
                _serverConfigurationManagerMock.Object,
                _subtitleManagerMock.Object,
                _libraryManagerMock.Object,
                _fileSystemMock.Object,
                _loggerFactoryMock.Object,
                _namingOptionsMock.Object,
                _lyricManagerMock.Object,
                _mediaAttachmentRepositoryMock.Object,
                _mediaStreamRepositoryMock.Object);

            // Act
            var result = probeProvider.HasChanged(item, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasChanged_LyricFilesChanged_ReturnsTrue()
        {
            // Arrange
            var item = new Audio
            {
                Path = "path",
                LyricFiles = new[] { "lyric1" }
            };

            _lyricResolverMock.Setup(lr => lr.GetExternalFiles(item, _directoryServiceMock.Object, false))
                .Returns(new[] { new ExternalFile { Path = "lyric2" } });

            var probeProvider = new ProbeProvider(
                _mediaSourceManagerMock.Object,
                _mediaEncoderMock.Object,
                _blurayExaminerMock.Object,
                _localizationManagerMock.Object,
                _chapterManagerMock.Object,
                _serverConfigurationManagerMock.Object,
                _subtitleManagerMock.Object,
                _libraryManagerMock.Object,
                _fileSystemMock.Object,
                _loggerFactoryMock.Object,
                _namingOptionsMock.Object,
                _lyricManagerMock.Object,
                _mediaAttachmentRepositoryMock.Object,
                _mediaStreamRepositoryMock.Object);

            // Act
            var result = probeProvider.HasChanged(item, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasChanged_NoChanges_ReturnsFalse()
        {
            // Arrange
            var item = new Video
            {
                Path = "path",
                SubtitleFiles = new[] { "subtitle1" }
            };

            _subtitleResolverMock.Setup(sr => sr.GetExternalFiles(item, _directoryServiceMock.Object, false))
                .Returns(new[] { new ExternalFile { Path = "subtitle1" } });

            var probeProvider = new ProbeProvider(
                _mediaSourceManagerMock.Object,
                _mediaEncoderMock.Object,
                _blurayExaminerMock.Object,
                _localizationManagerMock.Object,
                _chapterManagerMock.Object,
                _serverConfigurationManagerMock.Object,
                _subtitleManagerMock.Object,
                _libraryManagerMock.Object,
                _fileSystemMock.Object,
                _loggerFactoryMock.Object,
                _namingOptionsMock.Object,
                _lyricManagerMock.Object,
                _mediaAttachmentRepositoryMock.Object,
                _mediaStreamRepositoryMock.Object);

            // Act
            var result = probeProvider.HasChanged(item, _directoryServiceMock.Object);

            // Assert
            Assert.False(result);
        }
    }

    public class ExternalFile
    {
        public string Path { get; set; }
    }
}
