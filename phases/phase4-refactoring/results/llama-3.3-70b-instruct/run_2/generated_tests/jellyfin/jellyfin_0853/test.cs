using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using Microsoft.Extensions.Logging;
using MediaBrowser.Providers;

namespace MediaBrowser.Providers.Tests
{
    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebugWhenSearchingForLyrics()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<MediaBrowser.Controller.Localization.ILocalizationManager>();

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            var audioItem = new Audio
            {
                Path = "path",
                Name = "name",
                Album = "album",
                AlbumArtists = new[] { "artist" },
                Artists = new[] { "artist" },
                RunTimeTicks = 100
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new[] { audioItem });

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
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<MediaBrowser.Controller.Localization.ILocalizationManager>();

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            var audioItem = new Audio
            {
                Path = "path",
                Name = "name",
                Album = "album",
                AlbumArtists = new[] { "artist" },
                Artists = new[] { "artist" },
                RunTimeTicks = 100
            };

            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new[] { audioItem });

            lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new[] { new RemoteLyricInfoDto { Id = "id" } });

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
