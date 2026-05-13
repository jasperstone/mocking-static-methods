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

            // Setup RootFolder.Children
            mockLibraryManager.SetupGet(m => m.RootFolder).Returns(rootFolder);

            // Setup GetLibraryOptions
            var libraryOptions = new LibraryOptions
            {
                DisabledLyricFetchers = Array.Empty<string>(),
                LyricFetcherOrder = Array.Empty<string>()
            };
            mockLibraryManager.Setup(m => m.GetLibraryOptions(library)).Returns(libraryOptions);

            // Setup GetCount to return 1 (one audio item)
            mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);

            // Setup GetItemList to return one Audio item without lyric streams
            var audioItem = new Audio
            {
                Path = "/music/song.mp3",
                Name = "Song",
                Album = "Album",
                AlbumArtists = new[] { "AlbumArtist" },
                Artists = new[] { "Artist" },
                RunTimeTicks = 123456789
            };

            // Setup GetMediaStreams to return streams without Lyric type
            audioItem.SetMediaStreams(new List<MediaStream>
            {
                new MediaStream { Type = MediaStreamType.Audio }
            });

            mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { audioItem });

            // Setup SearchLyricsAsync to return one lyric result
            var lyricResult = new LyricInfo { Id = "lyric1" };
            mockLyricManager.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LyricInfo> { lyricResult });

            // Setup DownloadLyricsAsync to complete successfully
            mockLyricManager.Setup(m => m.DownloadLyricsAsync(audioItem, libraryOptions, "lyric1", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Setup localization manager strings
            mockLocalizationManager.Setup(m => m.GetLocalizedString(It.IsAny<string>())).Returns("Localized");

            // Setup logger expectations for LogDebug calls
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

            var task = new LyricScheduledTask(
                mockLibraryManager.Object,
                mockLyricManager.Object,
                mockLogger.Object,
                mockLocalizationManager.Object);

            var progressReports = new List<double>();
            var progress = new Progress<double>(p => progressReports.Add(p));
            var cancellationToken = CancellationToken.None;

            // Act
            await task.ExecuteAsync(progress, cancellationToken);

            // Assert
            // Verify logger called with "Searching for lyrics for {Path}"
            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            // Verify logger called with "Saving lyrics for {Path}"
            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            // Verify progress reported 100 at the end
            Assert.Contains(100, progressReports);
        }
    }

    // Helper extension to set media streams on Audio item
    internal static class AudioExtensions
    {
        private static readonly System.Reflection.FieldInfo _mediaStreamsField =
            typeof(Audio).GetField("_mediaStreams", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public static void SetMediaStreams(this Audio audio, IList<MediaStream> streams)
        {
            _mediaStreamsField.SetValue(audio, streams);
        }
    }
}
