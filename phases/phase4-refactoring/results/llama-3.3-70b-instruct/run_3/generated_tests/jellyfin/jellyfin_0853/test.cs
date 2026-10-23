using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Providers.Tests
{
    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebugWhenSearchingForLyrics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            var audioItem = new Audio
            {
                Path = "/path/to/audio",
                Name = "Audio Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Artist1", "Artist2" },
                Artists = new[] { "Artist1", "Artist2" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { audioItem });

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsDebugWhenSavingLyrics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            var audioItem = new Audio
            {
                Path = "/path/to/audio",
                Name = "Audio Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Artist1", "Artist2" },
                Artists = new[] { "Artist1", "Artist2" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { audioItem });

            lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new[] { new RemoteLyricInfoDto { Id = "LyricId" } });

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(2));
        }
    }
}
