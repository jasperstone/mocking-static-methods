using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task ExecuteAsync_ShouldLogDebugAndDownloadLyrics()
        {
            // Arrange
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var localizationManagerMock = new Mock<ILocalizationManager>();

            var audioItem = new Audio
            {
                Path = "testPath",
                Name = "testName",
                Album = "testAlbum",
                AlbumArtists = new[] { "testArtist" },
                Artists = new[] { "testArtist" },
                RunTimeTicks = 1000
            };

            var lyricResults = new List<LyricResult>
            {
                new LyricResult { Id = "testId" }
            };

            libraryManagerMock.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            libraryManagerMock.Setup(lm => lm.RootFolder.Children).Returns(new List<Folder> { new Folder() });
            libraryManagerMock.Setup(lm => lm.GetLibraryOptions(It.IsAny<Folder>())).Returns(new LibraryOptions());
            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { audioItem });

            lyricManagerMock.Setup(lm => lm.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);

            var task = new LyricScheduledTask(
                libraryManagerMock.Object,
                lyricManagerMock.Object,
                loggerMock.Object,
                localizationManagerMock.Object);

            var progressMock = new Mock<IProgress<double>>();

            // Act
            await task.ExecuteAsync(progressMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("Searching for lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogDebug("Saving lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once);

            lyricManagerMock.Verify(
                lm => lm.DownloadLyricsAsync(audioItem, It.IsAny<LibraryOptions>(), "testId", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
