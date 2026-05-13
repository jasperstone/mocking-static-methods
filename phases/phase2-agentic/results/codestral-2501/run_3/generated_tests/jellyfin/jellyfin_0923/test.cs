using Xunit;
using MediaBrowser.Providers.MediaInfo;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<AudioResolver> _audioResolverMock;
        private readonly Mock<SubtitleResolver> _subtitleResolverMock;
        private readonly Mock<LyricResolver> _lyricResolverMock;
        private readonly Mock<FFProbeVideoInfo> _videoProberMock;
        private readonly Mock<AudioFileProber> _audioProberMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;

        public ProbeProviderTests()
        {
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _directoryServiceMock = new Mock<IDirectoryService>();
            _audioResolverMock = new Mock<AudioResolver>(MockBehavior.Strict, _loggerMock.Object, null, null, null, null);
            _subtitleResolverMock = new Mock<SubtitleResolver>(MockBehavior.Strict, _loggerMock.Object, null, null, null, null);
            _lyricResolverMock = new Mock<LyricResolver>(MockBehavior.Strict, _loggerMock.Object, null, null, null, null);
            _videoProberMock = new Mock<FFProbeVideoInfo>(MockBehavior.Strict, _loggerMock.Object, null, null, null, null, null, null, null, _audioResolverMock.Object, _subtitleResolverMock.Object, null, null);
            _audioProberMock = new Mock<AudioFileProber>(MockBehavior.Strict, _loggerMock.Object, null, null, _lyricResolverMock.Object, null, null, null);
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        }

        [Fact]
        public void HasChanged_ShouldLogDebug_WhenExternalLyricsChange()
        {
            // Arrange
            var audio = new Audio
            {
                LyricFiles = new[] { "lyric1.txt" },
                SupportsLocalMetadata = true
            };

            var externalFiles = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { Path = "lyric2.txt" }
            };

            _lyricResolverMock.Setup(x => x.GetExternalFiles(audio, _directoryServiceMock.Object, false)).Returns(externalFiles);

            var probeProvider = new ProbeProvider(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _libraryManagerMock.Object,
                null,
                _loggerFactoryMock.Object,
                null,
                _lyricResolverMock.Object,
                null,
                null);

            // Act
            var result = probeProvider.HasChanged(audio, _directoryServiceMock.Object);

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
    }
}
