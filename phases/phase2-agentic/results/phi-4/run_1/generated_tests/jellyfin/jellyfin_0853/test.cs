using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Lyric.Tests
{
    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebugMessage_WhenSavingLyrics()
        {
            // Arrange
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();

            var audioItem = new Audio
            {
                Path = "/path/to/audio",
                Name = "SongName",
                Album = "AlbumName",
                AlbumArtists = new List<string> { "AlbumArtist" },
                Artists = new List<string> { "Artist" },
                RunTimeTicks = TimeSpan.FromMinutes(3).Ticks
            };

            var libraryOptions = new LibraryOptions();
            var lyricResults = new List<LyricResult> { new LyricResult { Id = 1 } };

            libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { audioItem });

            lyricManagerMock.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);

            lyricManagerMock.Setup(m => m.DownloadLyricsAsync(It.IsAny<Audio>(), It.IsAny<LibraryOptions>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(
                m => m.LogDebug("Saving lyrics for {Path}", audioItem.Path),
                Times.Once);
        }
    }
}
