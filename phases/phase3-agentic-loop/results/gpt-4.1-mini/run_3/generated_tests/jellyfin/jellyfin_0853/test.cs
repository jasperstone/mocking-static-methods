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
            var mockLocalizationManager = new Mock<ILocalizationManager>(MockBehavior.Strict);

            var audioItem = new Mock<Audio>();
            audioItem.Setup(a => a.Path).Returns("path/to/audio");
            audioItem.Setup(a => a.Name).Returns("SongName");
            audioItem.Setup(a => a.Album).Returns("AlbumName");
            audioItem.Setup(a => a.AlbumArtists).Returns(new[] { "AlbumArtist" });
            audioItem.Setup(a => a.Artists).Returns(new[] { "Artist" });
            audioItem.Setup(a => a.RunTimeTicks).Returns(123456L);
            audioItem.Setup(a => a.GetMediaStreams()).Returns(new List<MediaStream> { new MediaStream { Type = MediaStreamType.Audio } });

            var library = new BaseItem { Id = "library1" };
            var rootFolder = new BaseItem
            {
                Children = new List<BaseItem> { library }
            };

            mockLibraryManager.Setup(lm => lm.RootFolder).Returns(rootFolder);
            mockLibraryManager.Setup(lm => lm.GetLibraryOptions(library)).Returns(new LibraryOptions());
            mockLibraryManager.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            mockLibraryManager.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { audioItem.Object });

            var lyricResult = new LyricInfo { Id = "lyricId" };
            var lyricResults = new List<LyricInfo> { lyricResult };

            mockLyricManager.Setup(lm => lm.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);
            mockLyricManager.Setup(lm => lm.DownloadLyricsAsync(audioItem.Object, It.IsAny<LibraryOptions>(), "lyricId", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

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

            mockLocalizationManager.Setup(lm => lm.GetLocalizedString(It.IsAny<string>())).Returns("Localized");

            var task = new LyricScheduledTask(
                mockLibraryManager.Object,
                mockLyricManager.Object,
                mockLogger.Object,
                mockLocalizationManager.Object);

            var progress = new Progress<double>();
            var cancellationToken = CancellationToken.None;

            // Act
            await task.ExecuteAsync(progress, cancellationToken);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for path/to/audio")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for path/to/audio")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
