using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Controller;

namespace MediaBrowser.Providers.Tests
{
    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebugMessage_WhenSearchingForLyrics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            var audioItem = new Audio
            {
                Path = "/path/to/audio/file.mp3",
                Name = "Song Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Artist Name" },
                Artists = new[] { "Artist Name" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new[] { audioItem });

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_LogsDebugMessage_WhenSavingLyrics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            var audioItem = new Audio
            {
                Path = "/path/to/audio/file.mp3",
                Name = "Song Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Artist Name" },
                Artists = new[] { "Artist Name" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new[] { audioItem });

            lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new[] { new RemoteLyricInfoDto { Id = "lyric-id", ProviderName = "provider-name", Lyrics = "lyrics" } });

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
