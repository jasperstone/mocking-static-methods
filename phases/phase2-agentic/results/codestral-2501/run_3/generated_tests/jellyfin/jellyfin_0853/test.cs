using Xunit;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Querying;
using System;

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
        public async Task ExecuteAsync_LogsDebug_WhenLyricsFound()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "testPath",
                Name = "testName",
                Album = "testAlbum",
                AlbumArtists = new List<string> { "testArtist" },
                Artists = new List<string> { "testArtist" },
                RunTimeTicks = 1000
            };

            var libraryOptions = new LibraryOptions();
            var itemQuery = new InternalItemsQuery();
            var progress = new Progress<double>();

            _libraryManagerMock.Setup(lm => lm.GetItemList(itemQuery)).Returns(new List<BaseItem> { audioItem });
            _libraryManagerMock.Setup(lm => lm.GetLibraryOptions(It.IsAny<Folder>())).Returns(libraryOptions);
            _lyricManagerMock.Setup(lm => lm.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RemoteLyricInfoDto> { new RemoteLyricInfoDto { Id = "testId" } });

            // Act
            await _lyricScheduledTask.ExecuteAsync(progress, CancellationToken.None);

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
