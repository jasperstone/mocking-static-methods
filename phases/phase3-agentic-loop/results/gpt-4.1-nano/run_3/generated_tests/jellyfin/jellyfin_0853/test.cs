using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests
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

            _localizationManagerMock.Setup(m => m.GetLocalizedString(It.IsAny<string>()))
                .Returns<string>(s => s);

            _task = new LyricScheduledTask(
                _libraryManagerMock.Object,
                _lyricManagerMock.Object,
                _loggerMock.Object,
                _localizationManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogDebug_WhenAudioItemHasNoLyrics()
        {
            // Arrange
            var progress = new Mock<IProgress<double>>();
            var cts = new CancellationTokenSource();

            var audioItem = new Audio
            {
                Path = "path/to/audio",
                Name = "Song",
                Album = "Album",
                AlbumArtists = new List<string> { "Artist" },
                Artists = new List<string> { "Artist" },
                RunTimeTicks = 123456
            };

            // Setup GetItemList to return our audio item
            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<Audio> { audioItem });

            // Setup SearchLyricsAsync to return a non-empty list
            _lyricManagerMock.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LyricsResult> { new LyricsResult { Id = "lyricId" } });

            // Setup DownloadLyricsAsync to do nothing
            _lyricManagerMock.Setup(m => m.DownloadLyricsAsync(It.IsAny<Audio>(), It.IsAny<LibraryOptions>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _task.ExecuteAsync(progress.Object, cts.Token);

            // Assert
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
