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
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLyricManager = new Mock<ILyricManager>();
            var mockLogger = new Mock<ILogger<LyricScheduledTask>>();
            var mockLocalizationManager = new Mock<ILocalizationManager>();

            var audioItem = new Audio
            {
                Path = "/path/to/audio",
                Name = "SongName",
                Album = "AlbumName",
                AlbumArtists = new List<string> { "AlbumArtist" },
                Artists = new List<string> { "Artist" },
                RunTimeTicks = TimeSpan.FromMinutes(3).Ticks
            };

            var audioItems = new List<BaseItem> { audioItem };
            mockLibraryManager
                .Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(audioItems);

            var lyricResults = new List<LyricResult>
            {
                new LyricResult { Id = 1 }
            };

            mockLyricManager
                .Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);

            var task = new LyricScheduledTask(mockLibraryManager.Object, mockLyricManager.Object, mockLogger.Object, mockLocalizationManager.Object);

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug("Saving lyrics for {Path}", audioItem.Path),
                Times.Once);
        }
    }
}
