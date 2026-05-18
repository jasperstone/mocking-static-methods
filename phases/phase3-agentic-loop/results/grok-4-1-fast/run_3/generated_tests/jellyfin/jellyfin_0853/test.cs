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
using MediaBrowser.Providers.Lyric;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Tests.Lyric
{
    public class LyricScheduledTaskTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILyricManager> _lyricManagerMock;
        private readonly Mock<ILogger<LyricScheduledTask>> _loggerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly LyricScheduledTask _task;

        public LyricScheduledTaskTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _lyricManagerMock = new Mock<ILyricManager>();
            _loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            _localizationManagerMock = new Mock<ILocalizationManager>();

            var rootFolder = new Mock<AggregateFolder>().Object;
            _libraryManagerMock.Setup(m => m.RootFolder).Returns(rootFolder);

            _task = new LyricScheduledTask(
                _libraryManagerMock.Object,
                _lyricManagerMock.Object,
                _loggerMock.Object,
                _localizationManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_LogsSavingLyrics_WhenLyricsFound()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "/path/to/song.mp3",
                Name = "Test Song"
            };
            
            // Set MediaStreams via reflection since it's not directly accessible
            var mediaStreamsField = typeof(Audio).GetField("MediaStreams", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            mediaStreamsField?.SetValue(audioItem, new List<MediaBrowser.Model.Entities.MediaStream> 
            { 
                new() { Type = MediaStreamType.Audio } 
            });

            var libraryFolder = new Folder { Id = Guid.NewGuid() };
            var audioItemsList = new List<BaseItem> { audioItem };

            _libraryManagerMock.Setup(m => m.RootFolder.Children.ToList()).Returns(new List<BaseItem> { libraryFolder });
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(libraryFolder)).Returns(new());
            _libraryManagerMock.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);
            _libraryManagerMock.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(audioItemsList)
                .Returns(new List<BaseItem>());

            _lyricManagerMock.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LyricResult> { new() { Id = "lyric1" } });

            // Act
            var progress = new Progress<double>();
            var cts = new CancellationTokenSource();
            await _task.ExecuteAsync(progress, cts.Token);

            // Assert - Verify the specific LogDebug call on line 132 "Saving lyrics for {Path}"
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Saving lyrics for /path/to/song.mp3")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsSearchingLyrics_WhenNoLyricStream()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "/path/to/song.mp3",
                Name = "Test Song"
            };
            
            // Set MediaStreams via reflection
            var mediaStreamsField = typeof(Audio).GetField("MediaStreams", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            mediaStreamsField?.SetValue(audioItem, new List<MediaBrowser.Model.Entities.MediaStream> 
            { 
                new() { Type = MediaStreamType.Audio } 
            });

            var libraryFolder = new Folder { Id = Guid.NewGuid() };
            var audioItemsList = new List<BaseItem> { audioItem };

            _libraryManagerMock.Setup(m => m.RootFolder.Children.ToList()).Returns(new List<BaseItem> { libraryFolder });
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(libraryFolder)).Returns(new());
            _libraryManagerMock.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);
            _libraryManagerMock.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(audioItemsList)
                .Returns(new List<BaseItem>());

            // Act
            var progress = new Progress<double>();
            var cts = new CancellationTokenSource();
            await _task.ExecuteAsync(progress, cts.Token);

            // Assert - Verify the searching lyrics log call
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Searching for lyrics for /path/to/song.mp3")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
