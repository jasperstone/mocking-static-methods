using Xunit;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Providers.Lyric.Tests
{
    public class LyricScheduledTaskTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILyricManager> _lyricManagerMock;
        private readonly Mock<ILogger<LyricScheduledTask>> _loggerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly LyricScheduledTask _lyricScheduledTask;

        public LyricScheduledTaskTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _lyricManagerMock = new Mock<ILyricManager>();
            _loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            _localizationManagerMock = new Mock<ILocalizationManager>();

            _lyricScheduledTask = new LyricScheduledTask(
                _libraryManagerMock.Object,
                _lyricManagerMock.Object,
                _loggerMock.Object,
                _localizationManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogDebug_WhenLyricsAreFound()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "testPath",
                Name = "testName",
                Album = "testAlbum",
                AlbumArtists = new[] { "testArtist" },
                Artists = new[] { "testArtist" },
                RunTimeTicks = 1000,
                MediaStreams = new List<MediaStream> { new MediaStream { Type = MediaStreamType.Video } }
            };

            var lyricResults = new List<RemoteLyricInfoDto>
            {
                new RemoteLyricInfoDto { Id = "testId" }
            };

            _libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { audioItem });

            _lyricManagerMock.Setup(lm => lm.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);

            _lyricManagerMock.Setup(lm => lm.DownloadLyricsAsync(It.IsAny<Audio>(), It.IsAny<LibraryOptions>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LyricDto());

            // Act
            await _lyricScheduledTask.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Searching for lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogDebug("Saving lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
