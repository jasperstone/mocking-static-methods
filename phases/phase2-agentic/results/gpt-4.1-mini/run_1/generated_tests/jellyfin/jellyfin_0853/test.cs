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
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Lyric.Tests
{
    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebugWhenSearchingAndSavingLyrics()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>(MockBehavior.Strict);
            var mockLyricManager = new Mock<ILyricManager>(MockBehavior.Strict);
            var mockLogger = new Mock<ILogger<LyricScheduledTask>>(MockBehavior.Strict);
            var mockLocalizationManager = new Mock<MediaBrowser.Model.Globalization.ILocalizationManager>(MockBehavior.Strict);

            var library = new BaseItem { Id = "lib1" };
            var rootFolder = new BaseItem { Children = new List<BaseItem> { library } };
            mockLibraryManager.SetupGet(m => m.RootFolder).Returns(rootFolder);

            mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);

            var libraryOptions = new LibraryOptions
            {
                DisabledLyricFetchers = Array.Empty<string>(),
                LyricFetcherOrder = Array.Empty<string>()
            };
            mockLibraryManager.Setup(m => m.GetLibraryOptions(library)).Returns(libraryOptions);

            var audioItem = new Audio
            {
                Path = "/music/song.mp3",
                Name = "Song",
                Album = "Album",
                AlbumArtists = new[] { "AlbumArtist" },
                Artists = new[] { "Artist" },
                RunTimeTicks = 1000,
                MediaStreams = new List<MediaStream>()
            };

            // MediaStreams returns empty or no Lyric type
            audioItem.MediaStreams.Add(new MediaStream { Type = MediaStreamType.Audio });

            var itemList = new List<BaseItem> { audioItem };
            mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(itemList);

            var lyricResult = new LyricInfo { Id = "lyric1" };
            var lyricResults = new List<LyricInfo> { lyricResult };

            mockLyricManager.Setup(m => m.SearchLyricsAsync(It.Is<LyricSearchRequest>(r =>
                r.MediaPath == audioItem.Path &&
                r.SongName == audioItem.Name &&
                r.AlbumName == audioItem.Album &&
                r.AlbumArtistsNames == audioItem.AlbumArtists &&
                r.ArtistNames == audioItem.Artists &&
                r.Duration == audioItem.RunTimeTicks &&
                r.IsAutomated == true &&
                r.DisabledLyricFetchers == libraryOptions.DisabledLyricFetchers &&
                r.LyricFetcherOrder == libraryOptions.LyricFetcherOrder
            ), It.IsAny<CancellationToken>())).ReturnsAsync(lyricResults);

            mockLyricManager.Setup(m => m.DownloadLyricsAsync(audioItem, libraryOptions, lyricResult.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Setup logger expectations for LogDebug calls
            mockLogger.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            mockLogger.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Setup logger to not throw on LogError (not expected here)
            mockLogger.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            var mockProgress = new Mock<IProgress<double>>();
            mockProgress.Setup(p => p.Report(It.IsAny<double>())).Verifiable();

            var task = new LyricScheduledTask(
                mockLibraryManager.Object,
                mockLyricManager.Object,
                mockLogger.Object,
                mockLocalizationManager.Object);

            var cts = new CancellationTokenSource();

            // Act
            await task.ExecuteAsync(mockProgress.Object, cts.Token);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            mockProgress.Verify(p => p.Report(It.IsAny<double>()), Times.AtLeastOnce);
        }
    }
}
