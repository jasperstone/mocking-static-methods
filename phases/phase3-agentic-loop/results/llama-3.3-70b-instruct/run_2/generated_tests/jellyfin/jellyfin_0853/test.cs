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
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
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
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();
            var progressMock = new Mock<IProgress<double>>();
            var cancellationTokenSource = new CancellationTokenSource();

            var audioItem = new Audio
            {
                Path = "/path/to/audio/item",
                Name = "Audio Item",
                Album = "Album",
                AlbumArtists = new[] { "Artist" },
                Artists = new[] { "Artist" },
                RunTimeTicks = 1000
            };

            libraryManagerMock.Setup(l => l.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { audioItem });
            libraryManagerMock.Setup(l => l.GetLibraryOptions(It.IsAny<BaseItem>())).Returns(new LibraryOptions());

            lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<LyricResult> { new LyricResult { Id = "lyricId" } });
            lyricManagerMock.Setup(l => l.DownloadLyricsAsync(It.IsAny<BaseItem>(), It.IsAny<LibraryOptions>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var task = new LyricScheduledTask(libraryManagerMock.Object, lyricManagerMock.Object, loggerMock.Object, localizationManagerMock.Object);

            // Act
            await task.ExecuteAsync(progressMock.Object, cancellationTokenSource.Token);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Saving lyrics for {Path}", audioItem.Path), Times.Once);
        }
    }
}
