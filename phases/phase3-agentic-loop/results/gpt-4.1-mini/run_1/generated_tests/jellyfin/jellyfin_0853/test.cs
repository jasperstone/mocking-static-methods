using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Lyrics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Lyric.Tests
{
    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebugForSearchingAndSavingLyrics()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLyricManager = new Mock<ILyricManager>();
            var mockLogger = new Mock<ILogger<LyricScheduledTask>>();
            var mockLocalizationManager = new Mock<MediaBrowser.Model.Globalization.ILocalizationManager>();

            var audioItemPath = "/music/song.mp3";

            var audioItem = new Mock<Audio>();
            audioItem.Setup(a => a.Path).Returns(audioItemPath);
            audioItem.Setup(a => a.Name).Returns("SongName");
            audioItem.Setup(a => a.Album).Returns("AlbumName");
            audioItem.Setup(a => a.AlbumArtists).Returns(new List<string> { "AlbumArtist" });
            audioItem.Setup(a => a.Artists).Returns(new List<string> { "Artist" });
            audioItem.Setup(a => a.RunTimeTicks).Returns(1000L);
            audioItem.Setup(a => a.GetMediaStreams()).Returns(new List<MediaStream>
            {
                new MediaStream { Type = MediaStreamType.Audio }
            });

            var library = new Mock<BaseItem>();
            var libraryOptions = new LibraryOptions
            {
                DisabledLyricFetchers = new List<string>(),
                LyricFetcherOrder = new List<string>()
            };

            mockLibraryManager.Setup(m => m.RootFolder).Returns(new BaseItem
            {
                Children = new List<BaseItem> { library.Object }
            });
            mockLibraryManager.Setup(m => m.GetLibraryOptions(library.Object)).Returns(libraryOptions);

            var audioItems = new List<BaseItem> { audioItem.Object };
            mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(audioItems);
            mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);

            var lyricResult = new LyricInfo { Id = "lyricId" };
            var lyricResults = new List<LyricInfo> { lyricResult };

            mockLyricManager.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);
            mockLyricManager.Setup(m => m.DownloadLyricsAsync(audioItem.Object, libraryOptions, "lyricId", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var task = new LyricScheduledTask(
                mockLibraryManager.Object,
                mockLyricManager.Object,
                mockLogger.Object,
                mockLocalizationManager.Object);

            var progress = new Progress<double>();

            // Act
            await task.ExecuteAsync(progress, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
