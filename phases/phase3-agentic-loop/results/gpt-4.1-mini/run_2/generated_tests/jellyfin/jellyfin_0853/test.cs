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
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Lyric.Tests
{
    // Minimal stub classes to simulate required types not available in repo
    public class AudioStub : Audio
    {
        private readonly List<MediaStream> _mediaStreams = new();

        // Match the base class signature exactly
        public override IReadOnlyList<MediaStream> GetMediaStreams() => _mediaStreams;

        public void AddMediaStream(MediaStream stream) => _mediaStreams.Add(stream);
    }

    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebugForSearchingAndSavingLyrics()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLyricManager = new Mock<ILyricManager>();
            var mockLogger = new Mock<ILogger<LyricScheduledTask>>();
            var mockLocalizationManager = new Mock<ILocalizationManager>();

            var library = new BaseItem();
            var rootFolder = new BaseItem { Children = new List<BaseItem> { library } };
            mockLibraryManager.Setup(m => m.RootFolder).Returns(rootFolder);
            mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);

            var libraryOptions = new LibraryOptions
            {
                DisabledLyricFetchers = Array.Empty<string>(),
                LyricFetcherOrder = Array.Empty<string>()
            };
            mockLibraryManager.Setup(m => m.GetLibraryOptions(library)).Returns(libraryOptions);

            var audioItem = new AudioStub
            {
                Path = "/music/song.mp3",
                Name = "Song",
                Album = "Album",
                AlbumArtists = new[] { "AlbumArtist" },
                Artists = new[] { "Artist" },
                RunTimeTicks = 1000
            };
            audioItem.AddMediaStream(new MediaStream { Type = MediaStreamType.Audio });

            var audioItems = new List<BaseItem> { audioItem };
            mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(audioItems);

            var lyricResult = new LyricInfo { Id = "lyric1" };
            var lyricResults = new List<LyricInfo> { lyricResult };

            mockLyricManager.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);

            mockLyricManager.Setup(m => m.DownloadLyricsAsync(audioItem, libraryOptions, lyricResult.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockLogger.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            mockLogger.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            mockLocalizationManager.Setup(l => l.GetLocalizedString(It.IsAny<string>())).Returns("Localized");

            var task = new LyricScheduledTask(
                mockLibraryManager.Object,
                mockLyricManager.Object,
                mockLogger.Object,
                mockLocalizationManager.Object);

            var progressReports = new List<double>();
            var progress = new Progress<double>(d => progressReports.Add(d));
            var cancellationToken = CancellationToken.None;

            // Act
            await task.ExecuteAsync(progress, cancellationToken);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for /music/song.mp3")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for /music/song.mp3")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            Assert.Contains(100, progressReports);
        }
    }
}
