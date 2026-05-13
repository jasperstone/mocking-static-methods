using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
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
            var progressMock = new Mock<IProgress<double>>();
            var cancellationTokenSource = new CancellationTokenSource();

            var audioItem = new Audio
            {
                Path = "/path/to/audio/file.mp3",
                Name = "Song Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Artist Name" },
                Artists = new[] { "Artist Name" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { audioItem });

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            // Act
            await task.ExecuteAsync(progressMock.Object, cancellationTokenSource.Token);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Searching for lyrics for {Path}", audioItem.Path), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsDebugWhenSavingLyrics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();
            var progressMock = new Mock<IProgress<double>>();
            var cancellationTokenSource = new CancellationTokenSource();

            var audioItem = new Audio
            {
                Path = "/path/to/audio/file.mp3",
                Name = "Song Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Artist Name" },
                Artists = new[] { "Artist Name" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { audioItem });

            lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new RemoteLyricInfoDto { Id = "lyric-id" } });

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            // Act
            await task.ExecuteAsync(progressMock.Object, cancellationTokenSource.Token);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Saving lyrics for {Path}", audioItem.Path), Times.Once);
        }
    }
}
