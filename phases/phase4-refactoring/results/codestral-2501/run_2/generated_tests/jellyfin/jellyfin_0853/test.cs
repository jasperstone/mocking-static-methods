using Xunit;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Globalization;

namespace MediaBrowser.Providers.Lyric.Tests
{
    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebug_WhenLyricsAreFound()
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
                AlbumArtists = new[] { "testAlbumArtist" },
                Artists = new[] { "testArtist" },
                RunTimeTicks = 1000
            };

            var libraryOptions = new LibraryOptions();
            var itemQuery = new InternalItemsQuery();
            var lyricResults = new List<RemoteLyricInfoDto> { new RemoteLyricInfoDto { Id = "testId", ProviderName = "testProvider", Lyrics = new LyricDto() } };

            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { audioItem });
            libraryManagerMock.Setup(lm => lm.GetLibraryOptions(It.IsAny<Folder>())).Returns(libraryOptions);
            lyricManagerMock.Setup(lm => lm.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(lyricResults);
            libraryManagerMock.Setup(lm => lm.RootFolder).Returns(new AggregateFolder());
            libraryManagerMock.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);

            var task = new LyricScheduledTask(
                libraryManagerMock.Object,
                lyricManagerMock.Object,
                loggerMock.Object,
                localizationManagerMock.Object
            );

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug("Saving lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once
            );
        }
    }
}
