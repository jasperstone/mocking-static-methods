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
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Providers.Lyric;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
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
            _localizationManagerMock.Setup(x => x.GetLocalizedString(It.IsAny<string>())).Returns("test");

            _task = new LyricScheduledTask(
                _libraryManagerMock.Object,
                _lyricManagerMock.Object,
                _loggerMock.Object,
                _localizationManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_LogsSavingLyrics_WhenLyricResultsFound()
        {
            // Arrange
            var audioItem = new Mock<Audio>();
            audioItem.Setup(a => a.Path).Returns("/test/song.mp3");
            audioItem.Setup(a => a.Name).Returns("Test Song");
            audioItem.Setup(a => a.Artists).Returns(new[] { "Test Artist" });
            audioItem.Setup(a => a.GetMediaStreams()).Returns(new[] { new MediaStream { Type = MediaStreamType.Audio } });
            var audioItemObj = audioItem.Object;

            var library = new Mock<AggregateFolder>();
            library.Setup(l => l.Children).Returns(new List<BaseItem>());
            _libraryManagerMock.Setup(m => m.RootFolder).Returns(library.Object);
            
            var libraryOptions = new Mock<ILibraryOptions>().Object;
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(It.IsAny<BaseItem>())).Returns(libraryOptions);
            
            _libraryManagerMock.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            
            var query = new InternalItemsQuery 
            { 
                Parent = library.Object,
                Limit = 100,
                StartIndex = 0
            };
            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new BaseItem[] { audioItemObj });

            var lyricResult = new LyricResponse();
            var lyricResults = new List<LyricResponse> { lyricResult };
            _lyricManagerMock.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);
            _lyricManagerMock.Setup(m => m.DownloadLyricsAsync(It.IsAny<Audio>(), It.IsAny<ILibraryOptions>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert - Verify LogDebug call on line 132
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Saving lyrics for {Path}") && ((string)v).Contains("/test/song.mp3")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
