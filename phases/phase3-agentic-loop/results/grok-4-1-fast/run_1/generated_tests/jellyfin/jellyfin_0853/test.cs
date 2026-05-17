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
            _mockLocalizationManager.Setup(x => x.GetLocalizedString(It.IsAny<string>())).Returns("Dummy");

            _task = new LyricScheduledTask(
                _mockLibraryManager.Object,
                _mockLyricManager.Object,
                _mockLogger.Object,
                _mockLocalizationManager.Object);
        }

        [Fact]
        public async Task ExecuteAsync_LogsSavingLyricsDebug_WhenLyricsFound()
        {
            // Arrange
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            var audioItem = new Audio
            {
                Path = "/test/path/song.mp3",
                Name = "Test Song"
            };

            // Mock no lyrics streams
            var mediaStreams = new List<MediaStream>();
            Mock.Get(audioItem).Setup(a => a.GetMediaStreams()).Returns(mediaStreams);

            var audioItems = new List<BaseItem> { audioItem };
            var rootFolder = new Folder();
            Mock.Get(rootFolder).Setup(f => f.Children).Returns(new List<BaseItem> { rootFolder });

            _mockLibraryManager
                .Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            _mockLibraryManager
                .SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(audioItems)
                .Returns(new List<BaseItem>());

            _mockLibraryManager
                .Setup(m => m.GetLibraryOptions(rootFolder))
                .Returns(new LibraryOptions());

            _mockLyricManager
                .Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), cancellationToken))
                .ReturnsAsync(new List<LyricResponse> 
                { 
                    new() 
                    { 
                        Stream = Array.Empty<byte>(),
                        Format = LyricFormat.Lrc
                    } 
                });

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert - Tests LogDebug("Saving lyrics for {Path}", audioItem.Path) on line 132
            _mockLogger.Verify(
                logger => logger.LogDebug(
                    "Saving lyrics for {Path}",
                    "/test/path/song.mp3"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsSearchingLyricsDebug_WhenNoLyricsStreams()
        {
            // Arrange
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            var audioItem = new Audio
            {
                Path = "/test/path/song.mp3",
                Name = "Test Song"
            };

            // Mock no lyrics streams
            var mediaStreams = new List<MediaStream>();
            Mock.Get(audioItem).Setup(a => a.GetMediaStreams()).Returns(mediaStreams);

            var audioItems = new List<BaseItem> { audioItem };
            var rootFolder = new Folder();
            Mock.Get(rootFolder).Setup(f => f.Children).Returns(new List<BaseItem> { rootFolder });

            _mockLibraryManager
                .Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            _mockLibraryManager
                .SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(audioItems)
                .Returns(new List<BaseItem>());

            _mockLibraryManager
                .Setup(m => m.GetLibraryOptions(rootFolder))
                .Returns(new LibraryOptions());

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert - Tests LogDebug("Searching for lyrics for {Path}", audioItem.Path)
            _mockLogger.Verify(
                logger => logger.LogDebug(
                    "Searching for lyrics for {Path}",
                    "/test/path/song.mp3"),
                Times.Once);
        }
    }
}
