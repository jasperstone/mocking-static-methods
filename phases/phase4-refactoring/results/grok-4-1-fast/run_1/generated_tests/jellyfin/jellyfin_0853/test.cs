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
using MediaBrowser.Providers.Lyric;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Lyric.Tests
{
    public class LyricScheduledTaskTests
    {
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<ILyricManager> _mockLyricManager;
        private readonly Mock<ILogger<LyricScheduledTask>> _mockLogger;
        private readonly Mock<ILocalizationManager> _mockLocalizationManager;
        private readonly LyricScheduledTask _task;

        public LyricScheduledTaskTests()
        {
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLyricManager = new Mock<ILyricManager>();
            _mockLogger = new Mock<ILogger<LyricScheduledTask>>();
            _mockLocalizationManager = new Mock<ILocalizationManager>();

            _task = new LyricScheduledTask(
                _mockLibraryManager.Object,
                _mockLyricManager.Object,
                _mockLogger.Object,
                _mockLocalizationManager.Object);
        }

        [Fact]
        public async Task ExecuteAsync_LogsSavingLyrics_WhenLyricResultsFound()
        {
            // Arrange
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();
            var audioItem = new Audio 
            { 
                Path = "/music/song.mp3", 
                Name = "Song",
                Artists = new[] { "Artist" }
            };
            
            // Mock GetMediaStreams to return no lyrics
            var mockMediaStreams = new Mock<IReadOnlyList<MediaStream>>();
            mockMediaStreams.Setup(x => x.All(s => s.Type != MediaStreamType.Lyric)).Returns(true);
            Mock.Get(audioItem).Setup(x => x.GetMediaStreams()).Returns(mockMediaStreams.Object);

            var library = new Folder { Name = "Library" };
            var libraryOptions = new MediaBrowser.Model.Configuration.LibraryOptions();

            _mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            _mockLibraryManager.Setup(m => m.RootFolder.Children).Returns(new[] { library });
            _mockLibraryManager.Setup(m => m.GetLibraryOptions(library)).Returns(libraryOptions);
            _mockLibraryManager.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new BaseItem[] { audioItem })
                .Returns(new BaseItem[0]);

            _mockLyricManager.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), cancellationToken))
                .ReturnsAsync(new List<RemoteLyricInfoDto> { new RemoteLyricInfoDto() });

            _mockLyricManager.Setup(m => m.DownloadLyricsAsync(
                It.IsAny<Audio>(), 
                libraryOptions, 
                It.IsAny<string>(), 
                cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert - Verify the specific LogDebug call on line 132
            _mockLogger.Verify(
                x => x.LogDebug(
                    "Saving lyrics for {Path}", 
                    "/music/song.mp3"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsSearchingLyrics_WhenNoLyricsFound()
        {
            // Arrange
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();
            var audioItem = new Audio { Path = "/music/song.mp3", Name = "Song" };
            
            // Mock GetMediaStreams to return no lyrics
            var mockMediaStreams = new Mock<IReadOnlyList<MediaStream>>();
            mockMediaStreams.Setup(x => x.All(s => s.Type != MediaStreamType.Lyric)).Returns(true);
            Mock.Get(audioItem).Setup(x => x.GetMediaStreams()).Returns(mockMediaStreams.Object);

            var library = new Folder { Name = "Library" };
            var libraryOptions = new MediaBrowser.Model.Configuration.LibraryOptions();

            _mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            _mockLibraryManager.Setup(m => m.RootFolder.Children).Returns(new[] { library });
            _mockLibraryManager.Setup(m => m.GetLibraryOptions(library)).Returns(libraryOptions);
            _mockLibraryManager.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new BaseItem[] { audioItem })
                .Returns(new BaseItem[0]);

            _mockLyricManager.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), cancellationToken))
                .ReturnsAsync(new List<RemoteLyricInfoDto>());

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert - Verify searching log was called but not saving log
            _mockLogger.Verify(
                x => x.LogDebug(
                    "Searching for lyrics for {Path}", 
                    "/music/song.mp3"),
                Times.Once);

            _mockLogger.Verify(
                x => x.LogDebug(
                    It.Is<string>(msg => msg.Contains("Saving lyrics")), 
                    It.IsAny<object[]>()),
                Times.Never);
        }
    }
}
