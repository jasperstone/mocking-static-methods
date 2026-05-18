using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Lyrics;

namespace MediaBrowser.Providers.Lyric.Tests
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
            var progressMock = new Mock<IProgress<double>>();
            var cancellationTokenSource = new CancellationTokenSource();

            var audioItem = new Audio
            {
                Path = "/path/to/audio",
                Name = "Audio Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Album Artist" },
                Artists = new[] { "Artist" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { audioItem });

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, null);

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
            var progressMock = new Mock<IProgress<double>>();
            var cancellationTokenSource = new CancellationTokenSource();

            var audioItem = new Audio
            {
                Path = "/path/to/audio",
                Name = "Audio Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Album Artist" },
                Artists = new[] { "Artist" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { audioItem });

            lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new RemoteLyricInfoDto { Id = "Lyric Id", ProviderName = "Provider Name", Lyrics = "Lyrics" } });

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, null);

            // Act
            await task.ExecuteAsync(progressMock.Object, cancellationTokenSource.Token);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Saving lyrics for {Path}", audioItem.Path), Times.Once);
        }
    }
}
