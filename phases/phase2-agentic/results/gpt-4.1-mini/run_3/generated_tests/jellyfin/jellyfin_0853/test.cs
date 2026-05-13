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
        public async Task ExecuteAsync_LogsDebugOnSearchingAndSavingLyrics()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>(MockBehavior.Strict);
            var mockLyricManager = new Mock<ILyricManager>(MockBehavior.Strict);
            var mockLogger = new Mock<ILogger<LyricScheduledTask>>(MockBehavior.Strict);
            var mockLocalizationManager = new Mock<MediaBrowser.Model.Globalization.ILocalizationManager>(MockBehavior.Strict);

            var library = new Mock<BaseItem>(MockBehavior.Strict, null, null, null);
            var rootFolder = new Mock<BaseItem>(MockBehavior.Strict, null, null, null);
            var children = new List<BaseItem> { library.Object };
            rootFolder.Setup(r => r.Children).Returns(children);

            mockLibraryManager.Setup(m => m.RootFolder).Returns(rootFolder.Object);
            mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            mockLibraryManager.Setup(m => m.GetLibraryOptions(library.Object)).Returns(new LibraryOptions
            {
                DisabledLyricFetchers = Array.Empty<string>(),
                LyricFetcherOrder = Array.Empty<string>()
            });

            var audioItem = new Mock<Audio>(MockBehavior.Strict, null, null, null);
            audioItem.Setup(a => a.Path).Returns("path/to/audio");
            audioItem.Setup(a => a.Name).Returns("SongName");
            audioItem.Setup(a => a.Album).Returns("AlbumName");
            audioItem.Setup(a => a.AlbumArtists).Returns(new[] { "AlbumArtist" });
            audioItem.Setup(a => a.Artists).Returns(new[] { "Artist" });
            audioItem.Setup(a => a.RunTimeTicks).Returns(123456L);
            audioItem.Setup(a => a.GetMediaStreams()).Returns(new List<MediaStream> { new MediaStream { Type = MediaStreamType.Audio } });

            var itemList = new List<BaseItem> { audioItem.Object };
            mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(itemList);

            var lyricResult = new LyricInfo { Id = "lyricId" };
            var lyricResults = new List<LyricInfo> { lyricResult };
            mockLyricManager.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);
            mockLyricManager.Setup(m => m.DownloadLyricsAsync(audioItem.Object, It.IsAny<LibraryOptions>(), "lyricId", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Setup logger expectations for LogDebug calls
            mockLogger.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for path/to/audio")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            mockLogger.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for path/to/audio")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var progressReports = new List<double>();
            var progress = new Progress<double>(d => progressReports.Add(d));

            var cancellationToken = CancellationToken.None;

            var task = new LyricScheduledTask(
                mockLibraryManager.Object,
                mockLyricManager.Object,
                mockLogger.Object,
                mockLocalizationManager.Object);

            // Act
            await task.ExecuteAsync(progress, cancellationToken);

            // Assert
            Assert.Contains(100, progressReports);
            mockLogger.VerifyAll();
            mockLyricManager.VerifyAll();
            mockLibraryManager.VerifyAll();
        }
    }
}
